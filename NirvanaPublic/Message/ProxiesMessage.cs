using System.Text.Json;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using Codexus.Cipher.Protocol;
using Codexus.Development.SDK.Entities;
using Codexus.Game.Launcher.Services.Java;
using Codexus.Game.Launcher.Utils;
using Codexus.Interceptors;
using Codexus.OpenSDK;
using Codexus.OpenSDK.Entities.Yggdrasil;
using NirvanaPublic.Entities;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Manager;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;
using Serilog;

namespace NirvanaPublic.Message;

public static class ProxiesMessage
{
    // 已启动代理
    private static readonly List<RunningProxy> ActiveProxies = [];

    /**
     * 启动本地代理
     * @param id 游戏服务器ID
     * @param name 玩家名称
     */
    public static async Task StartProxyAsync(string id, string name)
    {
        // 插件初始化
        PluginMessage.InitializeAuto();
        Log.Information("正在启动本地代理...");
        Log.Information("名称：{name}", name);

        if (InfoManager.GameAccount == null || InfoManager.GameUser == null)
            throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
        if (PublicProgram.Services == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServicesNotInitialized);
        try
        {
            lock (LockManager.ActiveProxiesLock)
            {
                // 关闭老代理
                var log = 0;
                foreach (var proxy in ActiveProxies.ToList()
                             .Where(proxy => proxy.Equals(InfoManager.GameUser, id, name)))
                {
                    log++;
                    proxy.Interceptor.ShutdownAsync();
                    ActiveProxies.Remove(proxy);
                }

                if (log > 0) Log.Information("已清理 {log} 个旧代理", log);
            }

            // 服务器普通信息
            var server = ServersGameMessage.GetServerId(id);
            if (server == null) throw new Code.ErrorCodeException(Code.ErrorCode.ServerInNot);

            // 服务器详细信息
            var details = await ServerInfoMessage.GetServerId2(server.EntityId);
            if (details == null) throw new Code.ErrorCodeException(Code.ErrorCode.DetailError);

            // 服务器地址
            var address = await ServerInfoMessage.GetServerAddress(server.EntityId);
            if (address == null) throw new Code.ErrorCodeException(Code.ErrorCode.AddressError);

            // 服务器版本
            var version = details.McVersionList[0]; // 1.20
            var gameVersion = GameVersionUtil.GetEnumFromGameVersion(version.Name);

            var serverModInfo = await InstallerService.InstallGameMods(
                InfoManager.GameUser.UserId,
                InfoManager.GameUser.AccessToken,
                gameVersion,
                new WPFLauncher(),
                server.EntityId,
                false);

            var mods = JsonSerializer.Serialize(serverModInfo);

            // 服务器角色信息
            var character = await ServerInfoMessage.GetUserName(server.EntityId, name);
            if (character == null) throw new Code.ErrorCodeException(Code.ErrorCode.NotFoundName);

            // 前往游戏
            InterConn.LoginStart(InfoManager.GameUser.UserId, InfoManager.GameUser.AccessToken).Wait();
            InterConn.GameStart(InfoManager.GameUser.UserId, InfoManager.GameUser.AccessToken, server.EntityId).Wait();

            // 创建代理 并 下载资源
            var interceptor =
                CreateProxyInterceptor(server, character, version, address, InfoManager.GameUser, mods);

            // 启动游戏
            await X19.InterconnectionApi.GameStartAsync(InfoManager.GameUser.UserId,
                InfoManager.GameUser.AccessToken, server.EntityId);

            // 增加代理
            lock (LockManager.ActiveProxiesLock)
            {
                var proxy = new RunningProxy(interceptor)
                {
                    UserId = InfoManager.GameAccount.UserId,
                    UserToken = InfoManager.GameUser.AccessToken,
                    ServerId = server.EntityId,
                    Nickname = character.Name
                };
                ActiveProxies.Add(proxy);
            }
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
        EntityAvailableUser availableUser,
        string mods)
    {
        return Interceptor.CreateInterceptor(
            new EntitySocks5 { Enabled = false },
            mods,
            server.EntityId,
            server.Name,
            version.Name,
            address.Ip,
            address.Port,
            character.Name,
            availableUser.UserId,
            availableUser.AccessToken,
            YggdrasilCallback
        );

        void YggdrasilCallback(string serverId)
        {
            Log.Warning("认证中: {serverId}", serverId);
            var signal = new SemaphoreSlim(0);
            Task.Run(async () =>
            {
                try
                {
                    if (PublicProgram.Services == null) throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
                    var pair = Md5Mapping.GetMd5FromGameVersion(version.Name);

                    var modsJson = JsonSerializer.Deserialize<ModList>(mods);
                    if (modsJson == null) throw new Code.ErrorCodeException(Code.ErrorCode.ModsError);

                    var success = await PublicProgram.Services.Yggdrasil.JoinServerAsync(new GameProfile
                    {
                        GameId = server.EntityId,
                        GameVersion = version.Name,
                        BootstrapMd5 = pair.BootstrapMd5,
                        DatFileMd5 = pair.DatFileMd5,
                        Mods = modsJson,
                        User = new UserProfile
                            { UserId = int.Parse(availableUser.UserId), UserToken = availableUser.AccessToken }
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