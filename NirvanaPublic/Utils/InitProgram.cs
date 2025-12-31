using System.Diagnostics;
using System.Text.Json;
using NirvanaPublic.Manager;
using NirvanaPublic.Message;
using NirvanaPublic.Utils.ViewLogger;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Yggdrasil;
using Serilog;
using Serilog.Events;
using WPFLauncherApi.Entities;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Utils;

public static class InitProgram
{
    private static Services? Services { get; set; }

    public static Services GetServices()
    {
        return Services ?? throw new ErrorCodeException(ErrorCode.ServicesNotInitialized);
    }

    public static void LogoInit()
    {
        // 清空框架信息
        Console.Clear();

        // 配置 Serilog 日志记录
        var logger = new Logger();
        logger.MinimumLevel.Information();
        logger.SetColor(LogEventLevel.Information, ConsoleColor.Yellow);
        logger.SetColor(LogEventLevel.Warning, ConsoleColor.DarkYellow);
        logger.SetColor(LogEventLevel.Error, ConsoleColor.Red);
        logger.SetColor(LogEventLevel.Fatal, ConsoleColor.DarkRed);
        Log.Logger = logger.CreateLogger();
    }

    public static void NelInit()
    {
        // Fantnel 服务器信息 初始化
        FantnelInit().Wait();

        // 插件初始化
        // 避免插件过早的加载，因为这是没必要的
        // await InitializeSystemComponentsAsync();

        // 版本安全检测
        VersionCheck();

        // 创建服务
        Services = CreateServices();
        Log.Information("------  完成 ------");

        // 默认登录
        // 避免 自动登录失败 被拉取
        if (!Debugger.IsAttached) AccountMessage.DefaultLogin(AccountMessage.GetAccountList());

        // 插件管理器初始化
        PluginMessage.Initialize();

        // 在线检测
        var onlineThread = new Thread(Online);
        onlineThread.Start();

        for (var i = 0; i < 4 && !PublicProgram.LatestVersion; i++) Log.Warning("当前版本不是最新版本，建议更新至最新版本，以获得更好的体验！");
    }

    /**
     * 版本安全检测
    */
    private static void VersionCheck()
    {
        // 检查是否为发布版本
        if (!PublicProgram.Release)
        {
            Log.Error("调试版，已跳过版本检测！");
            return;
        }

        if (InfoManager.FantnelInfo?.Versions == null)
        {
            Log.Error("该版本已被禁用，请前往 https://npyyds.top/ 查看最新版本！");
            Thread.Sleep(6000);
            Environment.Exit(1);
        }

        var isVersion = false; // 版本 是否存在
        foreach (var version in InfoManager.FantnelInfo.Versions)
            if (version == PublicProgram.Mode + PublicProgram.Version || version == "All" + PublicProgram.Version)
                isVersion = true;

        if (!isVersion)
        {
            Log.Error("该版本已被禁用，请前往 https://npyyds.top/ 查看最新版本！");
            Thread.Sleep(6000);
            Environment.Exit(1);
        }

        // 检查是否为最新版本
        if (InfoManager.FantnelInfo.Versions.Last().EndsWith(PublicProgram.Version)) return;
        PublicProgram.LatestVersion = false;
    }

    // Fantnel 在线检测
    private static async void Online(object? o)
    {
        try
        {
            while (true)
            {
                // 60 * 3 = 180 秒 (3分钟)
                for (var i = 0; i < 180; i++) await Task.Delay(1000);
                var httpClient = new HttpClient();
                await httpClient.PostAsync("http://110.42.70.32:13423/api/tick?mode=fantnel",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            { "system", PublicProgram.Mode },
                            { "version", PublicProgram.Version },
                            { "versionId", PublicProgram.VersionId.ToString() }
                        }
                    ));
            }
        }
        catch (Exception e)
        {
            Log.Warning(" 在线检测异常! 错误信息: {Exception}", e);
        }
    }

    // 将FantnelInit定义为类的静态方法
    private static async Task FantnelInit(bool exitOnError = true)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("http://110.42.70.32:13423/fantnel.json");
        var json = await response.Content.ReadAsStringAsync();
        var entity = JsonSerializer.Deserialize<EntityInfo>(json);
        if (entity == null)
        {
            if (!exitOnError) return;
            Log.Error("连接服务器失败!");
            Thread.Sleep(6000);
            Environment.Exit(1);
            return;
        }

        InfoManager.FantnelInfo = entity;
    }

    // 创建服务
    private static Services CreateServices()
    {
        if (InfoManager.FantnelInfo == null || InfoManager.FantnelInfo.CrcSalt == null ||
            InfoManager.FantnelInfo.GameVersion == null)
        {
            Log.Error("CRC Salt 计算失败!");
            Thread.Sleep(6000);
            Environment.Exit(1);
            return null;
        }

        Log.Information("CRC Salt 当前版本: {Version}", InfoManager.FantnelInfo.GameVersion);
        Log.Information("CRC Salt 计算完成: {CrcSalt}....", InfoManager.FantnelInfo.CrcSalt[..6]);

        var yggdrasil = new StandardYggdrasil(new YggdrasilData
        {
            LauncherVersion = X19.GameVersion,
            Channel = "netease",
            CrcSalt = InfoManager.FantnelInfo.CrcSalt
        });

        return new Services(yggdrasil);
    }
}