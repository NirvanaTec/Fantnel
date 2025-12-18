using Codexus.Development.SDK.Entities;
using Codexus.Interceptors;

namespace NirvanaPublic.Entities.NEL;

public class RunningProxy(Interceptor interceptor)
{
    public string? UserId { get; init; }
    public string? UserToken { get; init; }
    public string? ServerId { get; init; }
    public string? Nickname { get; init; }
    public Interceptor Interceptor { get; } = interceptor;

    /**
     * 清理 相同/过期 的代理
     * @param gameUser 游戏用户
     * @param serverId 服务器ID
     * @param nickname 昵称
     * @return 是否为同一个用户
     */
    public bool Equals(EntityAvailableUser? gameUser, string? serverId, string? nickname)
    {
        return Equals(gameUser?.UserId, serverId, nickname) || Equals(gameUser);
    }

    /**
     * 判断是否为同一个用户, 服务器, 昵称
     * 主要是为了清理相同的代理，避免重复启动
     * @param userId 用户ID
     * @param serverId 服务器ID
     * @param nickname 昵称
     * @return 是否为同一个用户
     */
    private bool Equals(string? userId, string? serverId, string? nickname)
    {
        return userId == UserId && serverId == ServerId && nickname == Nickname;
    }

    /**
     * 判断是否为同一个用户, 但是Token不同
     * 主要是为了清理过期的代理
     * @param userId 用户ID
     * @param userToken 用户Token
     * @return 是否为同一个用户
     */
    private bool Equals(EntityAvailableUser? gameUser)
    {
        return gameUser?.UserId == UserId && gameUser?.AccessToken != UserToken;
    }
}