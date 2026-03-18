using Nirvana.Chat.Message;
using Nirvana.Public.Entities.Nirvana;
using Nirvana.WPFLauncher.Http;
using NirvanaAPI;
using NirvanaAPI.Entities.EntitiesNirvana;
using NirvanaAPI.Utils;

namespace Nirvana.Public.Manager;

public class NirvanaAccountManager {
    // 登录账号
    public static async Task Login(string account, string password)
    {
        var entity = await X19Extensions.Nirvana.Api<EntityNirvanaLogin>("/api/login?mode=fantnel&account=" + account +
                                                                         "&password=" + password);
        if (entity == null) {
            throw new Exception();
        }

        if (string.IsNullOrEmpty(entity.Token)) {
            throw new Exception(entity.Msg);
        }

        NirvanaConfig.Config.Account = account;
        NirvanaConfig.Config.Token = entity.Token;
        NirvanaConfig.SaveConfig();
        await ChatMessage.AuthenticateAsync();
    }

    // 初始化
    public static void Initialization()
    {
        var entity = Tools.GetValueOrDefault<EntityNirvanaConfig>("nirvanaAccount.json").Item1;
        if (entity == null) {
            return;
        }

        NirvanaConfig.Config = entity;
    }

    // 获取信息
    public static async Task<EntityNirvanaConfig> GetInfo()
    {
        if (NirvanaConfig.IsLogin()) {
            return GetInfo1();
        }

        var entity =
            await X19Extensions.Nirvana.Api<EntityNirvanaInfo>("/api/info?mode=fantnel" + NirvanaConfig.Config);
        if (entity == null) {
            throw new Exception();
        }

        NirvanaConfig.Config.Days = entity.Days;
        return GetInfo1();
    }

    // 获取信息
    private static EntityNirvanaConfig GetInfo1()
    {
        var config = new EntityNirvanaConfig {
            Account = NirvanaConfig.Config.Account,
            Days = NirvanaConfig.Config.Days,
            HideAccount = NirvanaConfig.Config.HideAccount,
            Token = NirvanaConfig.Config.Token
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

    public static void SetChatEnable(string value)
    {
        NirvanaConfig.SetChatEnable(value);
        if ("true".Equals(value, StringComparison.OrdinalIgnoreCase)) {
            _ = ChatMessage.StartAsync();
        } else {
            ChatMessage.Shutdown();
        }
    }
}