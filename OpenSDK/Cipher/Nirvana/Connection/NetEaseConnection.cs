using System.Text.Json;
using OpenSDK.Entities.Yggdrasil;
using OpenSDK.Yggdrasil;
using Serilog;

namespace OpenSDK.Cipher.Nirvana.Connection;

public static class NetEaseConnection
{
    public static async Task CreateAuthenticatorAsync(string serverId, string gameId, string gameVersion,
        string modInfo, int userId, string userToken, Action handleSuccess)
    {
        Log.Warning("认证中: {serverId}", serverId);
        var yggdrasil = new StandardYggdrasil();
        var pair = Md5Mapping.GetMd5FromGameVersion(gameVersion);
        var success = await yggdrasil.JoinServerAsync(new GameProfile
        {
            GameId = gameId,
            GameVersion = gameVersion,
            BootstrapMd5 = pair.BootstrapMd5,
            DatFileMd5 = pair.DatFileMd5,
            Mods = JsonSerializer.Deserialize<ModList>(modInfo),
            User = new UserProfile
            {
                UserId = userId,
                UserToken = userToken
            }
        }, serverId);
        if (success.IsSuccess)
        {
            Log.Information("认证完成!");
            handleSuccess();
        }
        else
        {
            Log.Error("认证失败: {Error}", success.Error);
        }
    }
}