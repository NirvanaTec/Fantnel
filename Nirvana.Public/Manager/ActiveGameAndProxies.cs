using System.Diagnostics;
using Codexus.Interceptors;
using Nirvana.Game.Launcher.Entities;
using Nirvana.Game.Launcher.Services.Java;
using Nirvana.Public.Entities.NEL;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Manager;
using Serilog;

namespace Nirvana.Public.Manager;

public static class ActiveGameAndProxies {
    // 线程安全锁
    private static readonly Lock SafeLock = new();

    /**
     * 已启动代理
     * 只记录 StartProxyAsync 方式的代理
     */
    private static readonly List<RunningProxy> ActiveProxies = [];

    /**
     * 已启动代理
     * 只记录 StartProxyAsync1 方式的代理
     */
    private static readonly List<EntityProxy> ActiveProxies1 = [];

    // 已启动白端游戏
    private static readonly List<LauncherService> ActiveLaunchers = [];

    /**
     * 清理已启动代理和游戏
     * @param gameId 服务器ID
     * @param name 角色名称
     */
    public static void Close(string gameId, string name)
    {
        lock (SafeLock) {
            // 关闭老代理
            var log = 0;
            foreach (var proxy in ActiveProxies.ToList().Where(proxy => proxy.Equals(InfoManager.GetGameAccount(), gameId, name))) {
                log++;
                proxy.Shutdown();
                ActiveProxies.Remove(proxy);
            }

            foreach (var proxy in ActiveProxies1.ToList().Where(proxy => proxy.Equals(InfoManager.GetGameAccount(), gameId, name))) {
                log++;
                proxy.Shutdown();
                ActiveProxies1.Remove(proxy);
            }

            if (log > 0) {
                Log.Information("已清理 {0} 个旧代理", log);
            }

            // 关闭老游戏
            log = 0;
            foreach (var launcher in ActiveLaunchers.ToList().Where(launcher => launcher.Entity.Equals(InfoManager.GetGameAccount(), gameId, name))) {
                log++;
                launcher.ShutdownAsync();
                ActiveLaunchers.Remove(launcher);
            }

            if (log > 0) {
                Log.Information("已清理 {0} 个旧游戏", log);
            }
        }
    }

    // 添加已启动代理1
    public static List<EntityLaunchGame> GetAllLaunchers()
    {
        // 获取已启动白端游戏
        DisposeActive();
        List<EntityLaunchGame> list = [];
        list.AddRange(ActiveLaunchers.Select(launcher => launcher.Entity));
        return list;
    }

    // 添加已启动代理
    public static RunningProxy Add(Interceptor interceptor, string serverId)
    {
        lock (SafeLock) {
            var proxy = new RunningProxy {
                Id = GetIndex(),
                UserId = InfoManager.GetGameAccount().UserId,
                UserToken = InfoManager.GetGameAccount().Token,
                ServerId = serverId,
                Interceptor = interceptor
            };
            ActiveProxies.Add(proxy);
            DisposeActive();
            return proxy;
        }
    }

    // 添加已启动代理1
    public static async Task<EntityProxy> Add(Process proxy, string serverId, string name, int port)
    {
        // 服务器地址
        var address = await NPFLauncher.GetNetGameServerAddressAsync(serverId);

        // 服务器详细信息
        var details = await NPFLauncher.GetNetGameDetailByIdAsync(serverId);

        lock (SafeLock) {
            var interceptor = new EntityProxyItem {
                NickName = name,
                LocalPort = port,
                ForwardAddress = address.Host,
                ForwardPort = address.Port,
                ServerName = details.Name,
                ServerVersion = details.McVersionList[0].Name
            };
            var entityProxy = new EntityProxy {
                Id = GetIndex(),
                UserId = InfoManager.GetGameAccount().UserId,
                UserToken = InfoManager.GetGameAccount().Token,
                ServerId = serverId,
                Interceptor = interceptor
            };
            entityProxy.SetProxy(proxy);
            ActiveProxies1.Add(entityProxy);
            DisposeActive();
            return entityProxy;
        }
    }

    // 添加已启动白端游戏
    public static void Add(LauncherService launcherService)
    {
        lock (SafeLock) {
            ActiveLaunchers.Add(launcherService);
            DisposeActive();
        }
    }

    // 关闭代理
    public static void CloseProxy(int id)
    {
        lock (SafeLock) {
            var proxy = ActiveProxies.FirstOrDefault(x => x.Id == id);
            if (proxy != null) {
                proxy.Interceptor.ShutdownAsync();
                ActiveProxies.Remove(proxy);
                Log.Information("已关闭代理 {0} ({1})", proxy.GetNickName(), proxy.Id);
                return;
            }

            var proxy1 = ActiveProxies1.FirstOrDefault(x => x.Id == id);
            if (proxy1 == null) {
                return;
            }

            proxy1.Shutdown();
            ActiveProxies1.Remove(proxy1);
            Log.Information("已关闭代理 {0} ({1})", proxy1.GetNickName(), proxy1.Id);
        }
    }

    // 关闭代理
    public static void CloseProxy(Interceptor interceptor)
    {
        lock (SafeLock) {
            var proxy = ActiveProxies.FirstOrDefault(x => x.Interceptor == interceptor);
            if (proxy != null) {
                proxy.Interceptor.ShutdownAsync();
                ActiveProxies.Remove(proxy);
                Log.Information("已关闭代理 {0} ({1})", proxy.GetNickName(), proxy.Id);
            }
        }
    }

    // 关闭白端游戏
    public static void CloseGame(int id)
    {
        lock (SafeLock) {
            var launcher = GetLauncherService(id);
            if (launcher == null) return;
            ActiveLaunchers.Remove(launcher);
            launcher.ShutdownAsync();
            Log.Information("白端游戏 {0} 已关闭", launcher.GetPid());
        }
    }

    // 获取白端游戏
    private static LauncherService? GetLauncherService(int id)
    {
        return ActiveLaunchers.FirstOrDefault(launcher => id.Equals(launcher.Entity.Id));
    }

    // 获取所有已启动代理
    public static List<object> GetAllProxies()
    {
        DisposeActive();
        lock (SafeLock) {
            var list = new List<object>();
            list.AddRange(ActiveProxies);
            list.AddRange(ActiveProxies1);
            return list;
        }
    }

    public static int GetIndex()
    {
        lock (SafeLock) {
            var max = ActiveProxies.Select(proxy => proxy.Id).Prepend(0).Max();
            max = ActiveProxies1.Select(proxy => proxy.Id).Prepend(max).Max();
            max = ActiveLaunchers.Select(proxy => proxy.Entity.Id).Prepend(max).Max();
            return max + 1;
        }
    }

    // 清理过期游戏白端
    private static void DisposeActive()
    {
        lock (SafeLock) {
            foreach (var launcher in ActiveLaunchers.ToList().Where(launcher => !launcher.IsRunning())) {
                Log.Information("白端游戏 {0} 已清理", launcher.GetPid());
                ActiveLaunchers.Remove(launcher);
                launcher.ShutdownAsync().Wait();
            }

            foreach (var launcher in ActiveProxies1.ToList().Where(launcher => !launcher.IsRunning())) {
                Log.Information("游戏代理 {0} 已清理", launcher.GetRunningPid());
                ActiveProxies1.Remove(launcher);
                launcher.Shutdown();
            }
        }
    }
}