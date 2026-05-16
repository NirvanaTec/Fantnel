using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nirvana.Cipher.Entities.Yggdrasil;
using Nirvana.Cipher.Yggdrasil;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI.Entities;
using NirvanaAPI.Entities.Login;
using Serilog;

namespace Nirvana.Cipher.Cipher.Nirvana.Connection;

public static class NetEaseConnection {
    // 认证失败 后通过 涅槃云 认证
    public static bool IsServerAuthenticated = true;

    public static void CreateAuthenticator(string serverId, string gameId, string gameVersion, string modInfo, EntityUserInfo userInfo, Action<bool> handle)
    {
        Task.Run(() => {
            try {
                CreateAuthenticatorAsync(serverId, gameId, gameVersion, modInfo, userInfo, handle).Wait();
            } catch (Exception) {
                // ignored
            }
        }).Wait();
    }

    public static async Task CreateAuthenticatorAsync(string serverId, string gameId, string gameVersion, string modInfo, EntityUserInfo userInfo, Action<bool> handle)
    {
        var pair = Md5Mapping.GetMd5FromGameVersion(gameVersion);
        var success = await CreateAuthenticatorAsync(new GameProfile {
            GameId = gameId,
            GameVersion = gameVersion,
            BootstrapMd5 = pair.BootstrapMd5,
            DatFileMd5 = pair.DatFileMd5,
            Mods = JsonSerializer.Deserialize<ModList>(modInfo),
            User = new UserProfile {
                User = userInfo
            }
        }, serverId);
        handle.Invoke(success);
    }

    public static void CreateAuthenticator(GameProfile gameProfile, string serverId)
    {
        CreateAuthenticatorAsync(gameProfile, serverId).GetAwaiter().GetResult();
    }

    private static async Task<bool> CreateAuthenticatorAsync(GameProfile gameProfile, string serverId)
    {
        Log.Warning("认证中: {0}", serverId);
        Exception? exception;
        try {
            await StandardYggdrasil.JoinServerAsync(gameProfile, serverId);
            Log.Information("认证完成!");
            return true;
        } catch (Exception e) {
            exception = e;
            Log.Error("认证失败: {0}", e.Message);
        }
        if (IsServerAuthenticated) {
            Log.Warning("代理认证中: {0}", serverId);
            var data = await X19Extensions.Nirvana.Api<EntityResponseBase>($"/api/fantnel/authenticated?id={serverId}", gameProfile);
            if (data == null) {
                Log.Error("代理认证出错！");
                return false;
            }
            if (data.Code == 1) {
                Log.Information("代理认证成功!");
                return true;
            }
            Log.Information("代理认证失败: {0}", data.Message);
        }
        throw exception;
    }
}