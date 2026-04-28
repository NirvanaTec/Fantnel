using Nirvana.Chat.Message;
using Nirvana.Public.Entities.Nirvana;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Utils.CodeTools;

namespace Nirvana.Public.Manager;

public static class NirvanaAccountManager {
    private static double? _days;

    // 登录账号
    public static async Task Login(string account, string password)
    {
        var entity = await X19Extensions.Nirvana.Api<EntityNirvanaLogin>("/api/login?mode=fantnel&account=" + account + "&password=" + password);
        if (entity == null) {
            throw new Exception();
        }

        if (string.IsNullOrEmpty(entity.Token)) {
            throw new Exception(entity.Msg);
        }

        _days = null;
        NirvanaConfig.SetValue("account", account);
        NirvanaConfig.SetValue("token", entity.Token);
        await ChatMessage.AuthenticateAsync();
    }

    // 获取信息
    public static async Task<EntityAccountNirvanaConfig> GetLoginInfo()
    {
        NirvanaConfig.IsLogin(); // 检查是否登录

        if (_days == null) {
            var entity = await X19Extensions.Nirvana.Api<EntityNirvanaInfo>("/api/info?mode=fantnel&" + NirvanaConfig.GetLoginT());
            if (entity == null) {
                throw new Exception();
            }

            if (entity.Code == 22) {
                NirvanaConfig.Logout();
                throw new ErrorCodeException(ErrorCode.OnlineStatusExpired);
            }

            _days = entity.Days;
        }

        var config = new EntityAccountNirvanaConfig {
            Account = NirvanaConfig.GetString("account"),
            Days = _days.Value,
            HideAccount = NirvanaConfig.GetBool("hideAccount")
        };

        if (config.HideAccount) {
            config.Account = MaskAccount(config.Account);
        }

        return config;
    }

    private static string MaskAccount(string account)
    {
        if (string.IsNullOrEmpty(account)) {
            return "*";
        }

        var length = account.Length;
        return length switch {
            // 例如: 1234567890123 -> 123****890123
            >= 13 => $"{account[..3]}****{account[(length - 3)..]}",
            // 例如: 123456789 -> 123***789
            >= 9 => $"{account[..3]}***{account[(length - 3)..]}",
            // 例如: 123456 -> 123***
            >= 6 => $"{account[..3]}***",
            // 例如: 12345 -> 1234*
            >= 5 => $"{account[..4]}*",
            // 例如: 1234 -> 123*
            >= 4 => $"{account[..3]}*",
            _ => "*"
        };
    }

    public static void SetChatEnable(string? value)
    {
        NirvanaConfig.SetValue("chatEnable", value);
        if (NirvanaConfig.GetBool("chatEnable")) {
            _ = ChatMessage.StartAsync();
        } else {
            ChatMessage.Shutdown();
        }
    }
}