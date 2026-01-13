using System.Diagnostics;

namespace NirvanaPublic.Entities.NEL;

public class EntityProxy: EntityProxyBase
{

    private Process? Proxy { get; set; }
    public required EntityProxyItem Interceptor { get; init; }
    
    /**
     * 获取游戏昵称
     */
    public override string GetNickName()
    {
       return Interceptor.NickName;
    }
    
    /**
     * 关闭代理
     */
    public void Kill()
    {
        Proxy?.Kill();
    }
    
    /**
     * 设置代理进程
     */
    public void SetProxy(Process proxy)
    {
        Proxy = proxy;
    }
    
}