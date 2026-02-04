using System.Diagnostics;

namespace NirvanaPublic.Entities.NEL;

public class EntityProxy : EntityProxyBase {
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
    public void Shutdown()
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

    /**
     * 是否运行中
     * @return 真:正在运行
     */
    public bool IsRunning()
    {
        return Proxy is { HasExited: false };
    }

    /**
     * 是否运行中
     * @return 真:正在运行
     */
    public int GetRunningPid()
    {
        return Proxy?.Id ?? -1;
    }
}