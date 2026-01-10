using Codexus.Game.Launcher.Services.Java;
using Codexus.Interceptors;
using NirvanaPublic.Entities.NEL;
using NirvanaPublic.Manager;
using Serilog;

namespace NirvanaPublic.Message;

public static class ActiveGameAndProxies
{
    // 线程安全锁
    private static readonly Lock SafeLock = new();

    // 已启动代理
    private static readonly List<RunningProxy> ActiveProxies = [];

    // 已启动白端游戏
    public static readonly List<LauncherService> ActiveLaunchers = [];

    /**
     * 清理已启动代理和游戏
     * @param id 服务器ID
     * @param name 角色名称
     */
    public static void Close(string id, string name)
    {
        lock (SafeLock)
        {
            // 关闭老代理
            var log = 0;
            foreach (var proxy in ActiveProxies.ToList()
                         .Where(proxy => proxy.Equals(InfoManager.GetGameAccount(), id, name)))
            {
                log++;
                proxy.Interceptor.ShutdownAsync();
                ActiveProxies.Remove(proxy);
            }

            if (log > 0) Log.Information("已清理 {log} 个旧代理", log);
            // 关闭老游戏
            log = 0;
            foreach (var launcher in ActiveLaunchers.ToList()
                         .Where(launcher => launcher.Entity.Equals(InfoManager.GetGameAccount(), id, name)))
            {
                log++;
                launcher.ShutdownAsync();
                ActiveLaunchers.Remove(launcher);
            }

            if (log > 0) Log.Information("已清理 {log} 个旧游戏", log);
        }
    }

    // 添加已启动代理
    public static void Add(Interceptor interceptor, string serverId)
    {
        lock (SafeLock)
        {
            var proxy = new RunningProxy(interceptor)
            {
                Id = ActiveProxies.Count + 1,
                UserId = InfoManager.GetGameAccount().UserId,
                UserToken = InfoManager.GetGameAccount().Token,
                ServerId = serverId
            };
            ActiveProxies.Add(proxy);
        }
    }

    // 添加已启动白端游戏
    public static void Add(LauncherService launcherService)
    {
        lock (SafeLock)
        {
            ActiveLaunchers.Add(launcherService);
        }
    }

    // 关闭代理
    public static void CloseProxy(int id)
    {
        lock (SafeLock)
        {
            var proxy = ActiveProxies.FirstOrDefault(x => x.Id == id);
            if (proxy == null) return;
            proxy.Interceptor.ShutdownAsync();
            ActiveProxies.Remove(proxy);
            Log.Information("已关闭代理 {Nickname} ({Id})", proxy.GetNickName(), proxy.Id);
        }
    }

    // 关闭白端游戏
    public static void CloseGame(string id)
    {
        lock (SafeLock)
        {
            var launcher = GetLauncherService(id);
            if (launcher == null) return;
            ActiveLaunchers.Remove(launcher);
            launcher.ShutdownAsync();
            Log.Information("白端游戏 {id} 已关闭", launcher.GetProcess().Id);
        }
    }

    // 获取白端游戏
    private static LauncherService? GetLauncherService(string id)
    {
        var index = 0;
        foreach (var launcher in ActiveLaunchers)
        {
            if (index.ToString().Equals(id)) return launcher;
            index++;
        }

        return null;
    }

    // 获取所有已启动代理
    public static List<RunningProxy> GetAllProxies()
    {
        lock (SafeLock)
        {
            return ActiveProxies;
        }
    }

    // 清理过期游戏白端
    public static void Dispose()
    {
        lock (SafeLock)
        {
            foreach (var launcher in ActiveLaunchers.ToList().Where(launcher => !launcher.IsRunning()))
            {
                Log.Information("白端游戏 {id} 已清理", launcher.GetProcess().Id);
                ActiveLaunchers.Remove(launcher);
                launcher.ShutdownAsync().Wait();
            }
        }
    }
}