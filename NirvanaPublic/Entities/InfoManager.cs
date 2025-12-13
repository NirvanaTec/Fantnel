using Codexus.Development.SDK.Entities;
using NirvanaPublic.Entities.Config;
using NirvanaPublic.Entities.Nirvana;

namespace NirvanaPublic.Entities;

public static class InfoManager
{
    public const string FantnelVersion = "1.0.0";

    // 涅槃 服务器 信息
    public static EntityInfo? FantnelInfo;

    // 游戏账号
    public static EntityAccount? GameAccount;
    public static EntityAvailableUser? GameUser;

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