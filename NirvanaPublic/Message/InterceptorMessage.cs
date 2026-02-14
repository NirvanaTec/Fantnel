using System.Text.Json;
using Codexus.Development.SDK.Entities;
using Codexus.Interceptors;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils;
using NirvanaAPI.Utils.CodeTools;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Yggdrasil;
using Serilog;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;

namespace NirvanaPublic.Message;

public class InterceptorMessage {
    private readonly EntityAccount _availableUser;

    private readonly string _entityId;
    private readonly string _mods;
    private readonly string _versionName;
    public readonly Interceptor Interceptor;

    public InterceptorMessage(
        EntityQueryNetGameDetailItem server,
        EntityGameCharacter character,
        EntityMcVersion version,
        EntityNetGameServerAddress address,
        string mods, int port)
    {
        _mods = mods;
        _versionName = version.Name;
        _entityId = server.EntityId;
        _availableUser = InfoManager.GetGameAccount();
        // 创建代理
        Interceptor = Interceptor.CreateInterceptor(
            new EntitySocks5 { Enabled = false },
            mods,
            server.EntityId,
            server.Name,
            version.Name,
            address.Host,
            address.Port,
            character.Name,
            _availableUser.GetUserId(),
            _availableUser.GetToken(),
            YggdrasilCallback,
            Tools.GetLocalIpAddress(false),
            port
        );
    }

    public InterceptorMessage(EntityRentalGameDetails server, EntityRentalGamePlayerList character, string versionName,
        EntityRentalGameServerAddress address, string mods, int port)
    {
        _mods = mods;
        _versionName = versionName;
        _entityId = server.EntityId;
        _availableUser = InfoManager.GetGameAccount();
        // 创建代理
        Interceptor = Interceptor.CreateInterceptor(
            new EntitySocks5 { Enabled = false },
            mods,
            server.EntityId,
            server.ServerName,
            versionName,
            address.McServerHost,
            address.McServerPort,
            character.Name,
            _availableUser.GetUserId(),
            _availableUser.GetToken(),
            YggdrasilCallback,
            Tools.GetLocalIpAddress(false),
            port
        );
    }

    private void YggdrasilCallback(string serverId)
    {
        Log.Warning("认证中: {serverId}", serverId);
        var signal = new SemaphoreSlim(0);
        Task.Run(async () => {
            try {
                var pair = Md5Mapping.GetMd5FromGameVersion(_versionName);

                var modsJson = JsonSerializer.Deserialize<ModList>(_mods);
                if (modsJson == null) throw new ErrorCodeException(ErrorCode.ModsError);

                var success = await StandardYggdrasil.JoinServerAsync(new GameProfile {
                    GameId = _entityId,
                    GameVersion = _versionName,
                    BootstrapMd5 = pair.BootstrapMd5,
                    DatFileMd5 = pair.DatFileMd5,
                    Mods = modsJson,
                    User = new UserProfile(_availableUser)
                }, serverId);

                if (success.IsSuccess) {
                    Log.Information("认证完成!");
                } else {
                    Log.Error("认证失败: {Error}", success.Error);
                    try {
                        if (!AccountMessage.AutoUpdateAccount(_availableUser)) {
                            ActiveGameAndProxies.CloseProxy(Interceptor);
                        }
                    } catch (Exception e) {
                        Log.Error("认证失败: {account}: {Message}", _availableUser.Account, e.Message);
                    }
                }
            } catch (Exception ex) {
                Log.Fatal("认证出错: {ex}", ex.Message);
            } finally {
                signal.Release();
            }
        });
        signal.Wait();
    }
}