using Codexus.Development.SDK.Entities;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Entities.Nirvana;
using NirvanaPublic.Utils.ViewLogger;

namespace NirvanaPublic.Manager;

public static class InfoManager
{
    // 涅槃 服务器 信息
    public static EntityInfo? FantnelInfo;

    // 游戏账号
    public static EntityAccount? GameAccount;
    public static EntityAvailableUser? GameUser;

    public static EntityAvailableUser GetGameUser()
    {
        return GameUser?.UserId == null ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot) : GameUser;
    }

    public static EntityAccount GetGameAccount()
    {
        return GameAccount?.UserId == null ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot) : GameAccount;
    }

    public static bool IsNotLogin()
    {
        // GameAccount == null || GameAccount.UserId == null
        return GameAccount?.UserId == null;
    }

    // public static X19AuthenticationOtp GetX19Au()
    // {
    //     return GameUser == null
    //         ? throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot)
    //         : new X19AuthenticationOtp { EntityId = GameUser.UserId, Token = GameUser.AccessToken };
    // }
}