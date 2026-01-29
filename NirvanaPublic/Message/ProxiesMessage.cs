using System.Text.Json;
using Codexus.Development.SDK.Entities;
using Codexus.Game.Launcher.Services.Java;
using Codexus.Game.Launcher.Utils;
using Codexus.Interceptors;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using OpenSDK.Entities.Config;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Yggdrasil;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;
using WPFLauncherApi.Protocol;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Message;

public static class ProxiesMessage
{

    /**
     * 启动本地代理
     * @param id 游戏服务器ID
     * @param name 玩家名称
     */
    public static async Task StartProxyAsync(string id, string name, string mode = "net")
    {
        Log.Information("正在启动本地代理...");
        Log.Information("名称：{name}", name);
        ActiveGameAndProxies.Close(id, name); // 清理旧代理
        // 子窗口的方式启动代理
        var port = Tools.GetUnusedPort(25565); // 获取没被占用的端口
        // 插件未初始化
        if (!PluginMessage.IsPluginChanged())
        {
            await StartProxyAsyncTo(id, name, port, mode);
            return;
        }

        List<string> arguments = ["--mode", "proxy", "--id", id, "--name", name];
        if (port != 25565)
        {
            arguments.Add("--port");
            arguments.Add(port.ToString());
        }

        if (mode != "net")
        {
            arguments.Add("--proxyMode");
            arguments.Add(mode);
        }

        // 避免 在 linux 上，控制台关闭导致，子进程控制台被隐藏
        arguments.Add("--MainPid");
        arguments.Add(Environment.ProcessId.ToString());

        var process = Tools.Restart(false, arguments);
        if (process == null) throw new ErrorCodeException(ErrorCode.RestartFailed);
        await ActiveGameAndProxies.Add(process, id, name, port);
    }

    /**
    * 启动本地代理 [真正的]
    * @param id 游戏服务器ID
    * @param name 玩家名称
    */
    public static async Task<int> StartProxyAsyncTo(string id, string name, int port = 25565, string mode = "net")
    {
        if ("rental".Equals(mode))
        {
            return await StartProxyAsyncRental(id, name, port);
        }
        return await StartProxyAsyncNet(id, name, port);
    }

    private static async Task<int> StartProxyAsyncNet(string id, string name, int port = 25565)
    {
        try
        {
            // 服务器详细信息
            var server = await WPFLauncher.GetNetGameDetailByIdAsync(id);

            // 服务器地址
            var address = await WPFLauncher.GetNetGameServerAddressAsync(server.EntityId);

            // 服务器版本
            var version = server.McVersionList[0]; // 1.20
            var gameVersion = GameVersionUtil.GetEnumFromGameVersion(version.Name);

            var serverModInfo = await InstallerService.InstallGameMods(
                gameVersion,
                server.EntityId);

            var mods = JsonSerializer.Serialize(serverModInfo);

            // 服务器角色信息
            var character = await ServersGameMessage.GetUserName(server.EntityId, name);
            if (character == null) throw new ErrorCodeException(ErrorCode.NotFoundName);

            // 前往游戏
            InterConn.LoginStart().Wait();

            // 启动游戏
            InterConn.GameStart(server.EntityId).Wait();
            // await X19.InterconnectionApi.GameStartAsync(server.EntityId);

            // 插件初始化
            PluginMessage.InitializeAuto();

            // 创建代理 并 下载资源
            var interceptor =
                 new InterceptorMessage(server, character, version, address, mods, port).Interceptor;

            // 增加代理
            ActiveGameAndProxies.Add(interceptor, server.EntityId);

            return interceptor.LocalPort;
        }
        catch (Exception ex)
        {
            Log.Error("启动代理失败：{ex}", ex.Message);
            throw;
        }
    }

    private static async Task<int> StartProxyAsyncRental(string id, string name, int port = 25565)
    {
        try
        {
            // 服务器详细信息
            var server = await WPFLauncher.GetRentalGameDetailsAsync(id);

            // 服务器地址
            var address = await WPFLauncher.GetGameRentalAddressAsync(server.EntityId);

            // 服务器版本
            var versionName = server.McVersion; // 1.20
            var gameVersion = GameVersionUtil.GetEnumFromGameVersion(versionName);

            var serverModInfo = await InstallerService.InstallGameMods(
                gameVersion,
                server.EntityId, true);

            var mods = JsonSerializer.Serialize(serverModInfo);

            // 服务器角色信息
            var character = await RentalGameMessage.GetUserName(server.EntityId, name);
            if (character == null) throw new ErrorCodeException(ErrorCode.NotFoundName);

            // 前往游戏
            InterConn.LoginStart().Wait();

            // 启动游戏
            InterConn.GameStart(server.EntityId).Wait();
            // await X19.InterconnectionApi.GameStartAsync(server.EntityId);

            // 插件初始化
            PluginMessage.InitializeAuto();

            // 创建代理 并 下载资源
            var interceptor =
                new InterceptorMessage(server, character, versionName, address, mods, port).Interceptor;

            // 增加代理
            ActiveGameAndProxies.Add(interceptor, server.EntityId);

            return interceptor.LocalPort;
        }
        catch (Exception ex)
        {
            Log.Error("启动代理失败：{ex}", ex.Message);
            throw;
        }
    }

}