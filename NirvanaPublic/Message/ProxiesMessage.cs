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
    public static async Task StartProxyAsync(string id, string name)
    {
        // 插件初始化
        Log.Information("正在启动本地代理...");
        Log.Information("名称：{name}", name);
        ActiveGameAndProxies.Close(id, name); // 清理旧代理
        var port = Tools.GetUnusedPort(25565); // 获取没被占用的端口
        List<string> arguments = ["--mode", "proxy", "--id", id, "--name", name];
        if (port != 25565)
        {
            arguments.Add("--port");
            arguments.Add(port.ToString());
        }
        var process = Tools.Restart(false, arguments);
        if (process == null) throw new ErrorCodeException(ErrorCode.RestartFailed);
        await ActiveGameAndProxies.Add(process, id, name, port);
    }

    /**
     * 启动本地代理 [真正的]
     * @param id 游戏服务器ID
     * @param name 玩家名称
     */
    public static async Task<int> StartProxyAsync1(string id, string name, int port = 25565)
    {
        try
        {
            // 服务器普通信息
            var server = ServersGameMessage.GetServerId(id);
            if (server == null) throw new ErrorCodeException(ErrorCode.ServerInNot);

            // 服务器详细信息
            var details = await WPFLauncher.QueryNetGameDetailByIdAsync(server.EntityId);

            // 服务器地址
            var address = await WPFLauncher.GetNetGameServerAddressAsync(server.EntityId);

            // 服务器版本
            var version = details.McVersionList[0]; // 1.20
            var gameVersion = GameVersionUtil.GetEnumFromGameVersion(version.Name);

            var serverModInfo = await InstallerService.InstallGameMods(
                gameVersion,
                server.EntityId);

            var mods = JsonSerializer.Serialize(serverModInfo);

            // 服务器角色信息
            var character = await ServerInfoMessage.GetUserName(server.EntityId, name);
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
                CreateProxyInterceptor(server, character, version, address, InfoManager.GetGameAccount(), mods, port);

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

    private static Interceptor CreateProxyInterceptor(
        EntityNetGameItem server,
        EntityGameCharacter character,
        EntityMcVersion version,
        EntityNetGameServerAddress address,
        EntityAccount availableUser,
        string mods, int port)
    {
        return Interceptor.CreateInterceptor(
            new EntitySocks5 { Enabled = false },
            mods,
            server.EntityId,
            server.Name,
            version.Name,
            address.Host,
            address.Port,
            character.Name,
            availableUser.GetUserId(),
            availableUser.GetToken(),
            YggdrasilCallback,
            Tools.GetLocalIpAddress(),
            port
        );

        void YggdrasilCallback(string serverId)
        {
            Log.Warning("认证中: {serverId}", serverId);
            var signal = new SemaphoreSlim(0);
            Task.Run(async () =>
            {
                try
                {
                    var pair = Md5Mapping.GetMd5FromGameVersion(version.Name);

                    var modsJson = JsonSerializer.Deserialize<ModList>(mods);
                    if (modsJson == null) throw new ErrorCodeException(ErrorCode.ModsError);

                    var success = await InitProgram.GetServices().Yggdrasil.JoinServerAsync(new GameProfile
                    {
                        GameId = server.EntityId,
                        GameVersion = version.Name,
                        BootstrapMd5 = pair.BootstrapMd5,
                        DatFileMd5 = pair.DatFileMd5,
                        Mods = modsJson,
                        User = new UserProfile
                            { UserId = int.Parse(availableUser.GetUserId()), UserToken = availableUser.GetToken() }
                    }, serverId);

                    if (success.IsSuccess)
                        Log.Information("认证完成!");
                    else
                        Log.Error("认证失败: {Error}", success.Error);
                }
                catch (Exception ex)
                {
                    Log.Fatal("认证出错: {ex}", ex.Message);
                }
                finally
                {
                    signal.Release();
                }
            });
            signal.Wait();
        }
    }
}