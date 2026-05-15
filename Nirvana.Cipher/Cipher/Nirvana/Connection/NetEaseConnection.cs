using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nirvana.Cipher.Entities.Yggdrasil;
using Nirvana.Cipher.Yggdrasil;
using NirvanaAPI.Entities.Login;
using Serilog;

namespace Nirvana.Cipher.Cipher.Nirvana.Connection;

public static class NetEaseConnection {

    public static void CreateAuthenticator(string serverId, string gameId, string gameVersion, string modInfo, EntityUserInfo userInfo, Action<bool> handle)
    {
        Task.Run(() => {
            CreateAuthenticatorAsync(serverId, gameId, gameVersion, modInfo, userInfo, handle).Wait();
        }).Wait();
    }

    public static async Task CreateAuthenticatorAsync(string serverId, string gameId, string gameVersion, string modInfo, EntityUserInfo userInfo, Action<bool> handle)
    {
        Log.Warning("认证中: {0}", serverId);
        var pair = Md5Mapping.GetMd5FromGameVersion(gameVersion);
        var success = await StandardYggdrasil.JoinServerAsync(new GameProfile {
            GameId = gameId,
            GameVersion = gameVersion,
            BootstrapMd5 = pair.BootstrapMd5,
            DatFileMd5 = pair.DatFileMd5,
            Mods = JsonSerializer.Deserialize<ModList>(modInfo),
            User = new UserProfile(userInfo)
        }, serverId);
        if (success.IsSuccess) {
            Log.Information("认证完成!");
            handle.Invoke(true);
        } else {
            Log.Error("认证失败: {0}", success.Error);
            handle.Invoke(false);
        }
    }
}