using Nirvana.Public.Manager;
using Nirvana.Public.Message;
using Nirvana.Public.Utils.Update;
using Nirvana.Public.Utils.ViewLogger;
using Nirvana.WPFLauncher.Http;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI;
using NirvanaAPI.Entities;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using Serilog;
using Serilog.Events;

namespace Nirvana.Public.Utils;

public static class InitProgram {
    public static void LogoInit()
    {
        // 配置 Serilog 日志记录
        var logger = new Logger();
        logger.MinimumLevel.Information();
        logger.SetColor(LogEventLevel.Information, ConsoleColor.Yellow);
        logger.SetColor(LogEventLevel.Warning, ConsoleColor.DarkYellow);
        logger.SetColor(LogEventLevel.Error, ConsoleColor.Red);
        logger.SetColor(LogEventLevel.Fatal, ConsoleColor.DarkRed);
        logger.SetColor(LogEventLevel.Debug, ConsoleColor.Cyan);
        logger.WriteTo.Sink(InMemorySink.Instance);
        Log.Logger = logger.CreateLogger();

        // 清空框架信息
        InMemorySink.Clear();
    }

    public static void NelInit(string[] args, Action logInit)
    {
        // 日志初始化
        logInit.Invoke();

        // 检查更新
        Log.Information("{0}", PathUtil.ResourcePath);
        UpdateTools.CheckUpdate(args).Wait();

        // 重置日志
        logInit.Invoke();
    }

    /**
     * 核心 初始化
     */
    public static void NelInit1()
    {
        // Fantnel 服务器信息 初始化
        FantnelInit().Wait();

        // 插件初始化
        // 避免插件过早的加载，因为这是没必要的
        // await InitializeSystemComponentsAsync();

        // 版本安全检测
        VersionCheck();

        // 创建服务
        CreateServices();
        Log.Information("------  完成 ------");

        // 配置初始化
        NirvanaConfig.Initialization();

        // 默认登录
        AccountMessage.GetAccountList();

        // 插件管理器初始化
        PluginMessage.Initialize();

        for (var i = 0; i < 4 && !PublicProgram.LatestVersion; i++) {
            Log.Warning("当前版本不是最新版本，建议更新至最新版本，以获得更好的体验！");
        }

        // 在线检测
        Online();

        // 缓存 服务器/租凭服/皮肤 信息/图片
        _ = Task.Run(() => {
            try {
                Thread.Sleep(1000);
                InfoManager.GetToken(); // 是否登录
                CacheManager.CacheServer();
            } catch (Exception) {
                // ignored
            }
        });
    }

    /**
     * 版本安全检测
    */
    private static void VersionCheck()
    {
        // 检查是否为发布版本
        if (!PublicProgram.Release) {
            Log.Error("调试版，已跳过版本检测！");
            return;
        }

        if (InfoManager.FantnelInfo?.Versions == null) {
            Log.Error("该版本已被禁用，请前往 https://npyyds.top/ 查看最新版本！");
            Thread.Sleep(6000);
            Environment.Exit(1);
        }

        var isVersion = false; // 版本 是否存在
        foreach (var version in InfoManager.FantnelInfo.Versions) {
            if (version == PublicProgram.Version) {
                isVersion = true;
            }
        }

        foreach (var version in InfoManager.FantnelInfo.DisabledVersions) {
            // x64_1.3.0
            if (version == PublicProgram.Arch + "_" + PublicProgram.Version) {
                isVersion = false;
            } else if (version == PublicProgram.Mode + "_" + PublicProgram.Arch + "_" + PublicProgram.Version) {
                // win_x64_1.3.0
                isVersion = false;
            } else if (version == PublicProgram.Mode + "_" + PublicProgram.Version) {
                // win_1.3.0
                isVersion = false;
            }
        }

        if (!isVersion) {
            Log.Error("该版本已被禁用，请前往 https://npyyds.top/ 查看最新版本！");
            Thread.Sleep(6000);
            Environment.Exit(1);
        }

        // 检查是否为最新版本
        if (InfoManager.FantnelInfo.Versions.Last().Equals(PublicProgram.Version)) {
            return;
        }

        PublicProgram.LatestVersion = false;
    }

    // Fantnel 在线检测
    private static async void Online()
    {
        try {
            while (true)
                try {
                    // 60 * 3 = 180 秒 (3分钟)
                    for (var i = 0; i < 180; i++) {
                        await Task.Delay(1000);
                    }
                    await X19Extensions.Nirvana.Api<EntityResponse<string>>("/api/tick?mode=fantnel",
                        new Dictionary<string, string> {
                            { "system", PublicProgram.Mode },
                            { "arch", PublicProgram.Arch },
                            { "version", PublicProgram.Version },
                            { "versionId", PublicProgram.VersionId.ToString() }
                        });
                } catch (Exception e) {
                    Log.Warning(" 在线检测异常! 错误信息: {0}", e.Message);
                }
        } catch (Exception e) {
            Log.Warning(" 在线检测出错! 错误信息: {0}", e.Message);
        }
    }

    private static async Task FantnelInit()
    {
        for (var i = 0; i < 3; i++) {
            try {
                var entity = await X19Extensions.Nirvana.Api<EntityInfo>("/fantnel.json");
                if (entity != null) {
                    InfoManager.FantnelInfo = entity;
                    return;
                }
            } catch (Exception e) {
                Log.Error("连接服务器失败! 错误信息: {0}", e.Message);
            }
        }

        Log.Error("连接服务器失败!");
        Thread.Sleep(6000);
        Environment.Exit(1);
    }

    // 创建服务
    private static void CreateServices()
    {
        if (InfoManager.FantnelInfo == null || InfoManager.FantnelInfo.CrcSalt == null ||
            InfoManager.FantnelInfo.GameVersion == null) {
            Log.Error("CRC Salt 计算失败!");
            Thread.Sleep(6000);
            Environment.Exit(1);
            return;
        }

        Log.Information("CRC Salt 当前版本: {0}", InfoManager.FantnelInfo.GameVersion);
        Log.Information("CRC Salt 计算完成: {0}....", InfoManager.FantnelInfo.CrcSalt[..6]);
        X19.CrcSalt = InfoManager.FantnelInfo.CrcSalt;
    }

    public static async Task<bool> SafeTheme(string themeValue)
    {
        return await X19Extensions.Nirvana.Api<EntityResponseBase>("/api/theme/name?value=" + themeValue) is { Code: 1 };
    }
}