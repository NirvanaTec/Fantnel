using OpenSDK.Entities.Config;
using WPFLauncherApi.Entities;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Manager;

public static class InfoManager
{
    public static readonly List<EntityAccount> GameAccountList = [];

    public static EntityAccount GetGameAccount()
    {
        return GameAccount?.UserId == null ? throw new ErrorCodeException(ErrorCode.LogInNot) : GameAccount;
    }

    public static bool IsNotLogin()
    {
        // GameAccount == null || GameAccount.UserId == null
        return GameAccount?.UserId == null;
    }

    public static void AddAccount(EntityAccount account)
    {
        // 账号已存在 | 移除旧账号
        foreach (var gameAccount in GameAccountList.Where(gameAccount => gameAccount.Equals(account)))
        {
            GameAccountList.Remove(gameAccount);
            break;
        }

        GameAccountList.Add(account);
        GameAccount ??= account;
    }
    // 涅槃 服务器 信息
#pragma warning disable CA2211
    public static EntityInfo? FantnelInfo;

    // 游戏账号
    public static EntityAccount? GameAccount;
#pragma warning restore CA2211

    // public static X19AuthenticationOtp GetX19Au()
    // {
    //     return GameUser == null
    //         ? throw new ErrorCodeException(ErrorCode.LogInNot)
    //         : new X19AuthenticationOtp { EntityId = GameUser.UserId, Token = GameUser.AccessToken };
    // }
}