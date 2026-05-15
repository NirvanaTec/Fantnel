using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nirvana.Cipher.Entities.Yggdrasil;
using Nirvana.Cipher.Yggdrasil;
using Nirvana.Development;
using Nirvana.DevPlugin.Entities;
using Nirvana.Heypixel;
using Nirvana.Public.Message;
using Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame;
using Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameCharacters;
using Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameDetails;
using Nirvana.WPFLauncher.Entities.WPFLauncher.RentalGame;
using Nirvana.WPFLauncher.Entities.WPFLauncher.RentalGame.GameCharacters;
using NirvanaAPI.Entities.Login;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace Nirvana.Public.Manager;

public class InterceptorManager {
    
    private readonly EntityAccount _availableUser;

    private readonly string _entityId;
    private readonly string _mods;
    private readonly string _versionName;
    public readonly Interceptor Interceptor;

    static InterceptorManager()
    {
        HeypixelProtocol.Init();
    }
    
    public InterceptorManager(EntityQueryNetGameDetailItem server, EntityGameCharacter character, EntityMcVersion version, EntityNetGameServerAddress address, string mods, int port)
    {
        _mods = mods;
        _versionName = version.Name;
        _entityId = server.EntityId;
        _availableUser = InfoManager.GetGameAccount();
        // 创建代理
        Interceptor = Interceptor.CreateInterceptor(false, mods, server.EntityId, server.Name, version.Name, address.Host, address.Port, character.Name, _availableUser, YggdrasilCallback, port);
    }

    public InterceptorManager(EntityRentalGameDetails server, EntityRentalGamePlayerList character, string versionName, EntityRentalGameServerAddress address, string mods, int port)
    {
        _mods = mods;
        _versionName = versionName;
        _entityId = server.EntityId;
        _availableUser = InfoManager.GetGameAccount();
        // 创建代理
        Interceptor = Interceptor.CreateInterceptor(true, mods, server.EntityId, server.ServerName, versionName, address.McServerHost, address.McServerPort, character.Name, _availableUser, YggdrasilCallback, port);
    }

    private void YggdrasilCallback(InterceptorConfig config, string serverId)
    {
        Log.Warning("认证中: {0}", serverId);
        Task.Run(async () => {
            try {
                var pair = Md5Mapping.GetMd5FromGameVersion(_versionName);

                var modsJson = JsonSerializer.Deserialize<ModList>(_mods);
                if (modsJson == null) {
                    throw new ErrorCodeException(ErrorCode.ModsError);
                }

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
                    Log.Error("认证失败: {0}", success.Error);
                    try {
                        AccountMessage.AutoUpdateAccount(_availableUser, () => { ActiveGameAndProxies.CloseProxy(Interceptor); });
                    } catch (Exception e) {
                        Log.Error("认证失败: {0}: {1}", _availableUser.Account, e.Message);
                    }
                }
            } catch (Exception ex) {
                Log.Fatal("认证出错: {0}", ex.Message);
            }
        }).Wait();
    }
}