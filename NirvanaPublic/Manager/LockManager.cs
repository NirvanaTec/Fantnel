namespace NirvanaPublic.Manager;

public static class LockManager
{
    // 保存/修改 游戏账号锁
    public static readonly Lock GameSaveAccountLock = new();

    // 登录游戏 锁
    public static readonly Lock LoginLock = new();

    // 保存/修改 游戏服务器列表 | 皮肤列表缓存 锁
    public static readonly Lock GameServerListLock = new();

    // 已启动代理 锁
    public static readonly Lock ActiveProxiesLock = new();

    // 插件状态 锁
    public static readonly Lock PluginStatesLock = new();

    // 插件列表缓存 锁
    public static readonly Lock PluginListLock = new();
}