using Codexus.Interceptors;

namespace NirvanaPublic.Entities.NEL;

public class RunningProxy : EntityProxyBase
{
    public required Interceptor Interceptor { get; init; }

    /**
     * 获取游戏昵称
     */
    public override string GetNickName()
    {
        return Interceptor.NickName;
    }
    
}