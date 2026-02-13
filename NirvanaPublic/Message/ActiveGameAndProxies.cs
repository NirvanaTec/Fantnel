using System.Diagnostics;
using Codexus.Game.Launcher.Entities;
using Codexus.Game.Launcher.Services.Java;
using Codexus.Interceptors;
using NirvanaAPI.Manager;
using NirvanaAPI.Utils.CodeTools;
using NirvanaPublic.Entities.NEL;
using Serilog;
using WPFLauncherApi.Protocol;

namespace NirvanaPublic.Message;

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
     * @param id 服务器ID
     * @param name 角色名称
     */
    public static void Close(string id, string name)
    {
        lock (SafeLock) {
            // 关闭老代理
            var log = 0;
            foreach (var proxy in ActiveProxies.ToList()
                         .Where(proxy => proxy.Equals(InfoManager.GetGameAccount(), id, name))) {
                log++;
                proxy.Shutdown();
                ActiveProxies.Remove(proxy);
            }

            foreach (var proxy in ActiveProxies1.ToList()
                         .Where(proxy => proxy.Equals(InfoManager.GetGameAccount(), id, name))) {
                log++;
                proxy.Shutdown();
                ActiveProxies1.Remove(proxy);
            }

            if (log > 0) Log.Information("已清理 {log} 个旧代理", log);

            // 关闭老游戏
            log = 0;
            foreach (var launcher in ActiveLaunchers.ToList()
                         .Where(launcher => launcher.Entity.Equals(InfoManager.GetGameAccount(), id, name))) {
                log++;
                launcher.ShutdownAsync();
                ActiveLaunchers.Remove(launcher);
            }

            if (log > 0) Log.Information("已清理 {log} 个旧游戏", log);
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
    public static void Add(Interceptor interceptor, string serverId)
    {
        lock (SafeLock) {
            var proxy = new RunningProxy {
                Id = GetProxiesIndex(),
                UserId = InfoManager.GetGameAccount().UserId,
                UserToken = InfoManager.GetGameAccount().Token,
                ServerId = serverId,
                Interceptor = interceptor
            };
            ActiveProxies.Add(proxy);
        }

        DisposeActive();
    }

    // 添加已启动代理1
    public static async Task Add(Process proxy, string serverId, string name, int port)
    {
        // 服务器地址
        var address = await WPFLauncher.GetNetGameServerAddressAsync(serverId);

        // 服务器普通信息
        var server = ServersGameMessage.GetServerById(serverId);
        if (server == null) throw new ErrorCodeException(ErrorCode.ServerInNot);

        // 服务器详细信息
        var details = await WPFLauncher.GetNetGameDetailByIdAsync(server.EntityId);

        lock (SafeLock) {
            var interceptor = new EntityProxyItem {
                NickName = name,
                LocalPort = port,
                ForwardAddress = address.Host,
                ForwardPort = address.Port,
                ServerName = server.Name,
                ServerVersion = details.McVersionList[0].Name
            };
            var entityProxy = new EntityProxy {
                Id = GetProxiesIndex(),
                UserId = InfoManager.GetGameAccount().UserId,
                UserToken = InfoManager.GetGameAccount().Token,
                ServerId = serverId,
                Interceptor = interceptor
            };
            entityProxy.SetProxy(proxy);
            ActiveProxies1.Add(entityProxy);
        }

        DisposeActive();
    }

    // 添加已启动白端游戏
    public static void Add(LauncherService launcherService)
    {
        lock (SafeLock) {
            ActiveLaunchers.Add(launcherService);
        }

        DisposeActive();
    }

    // 关闭代理
    public static void CloseProxy(int id)
    {
        lock (SafeLock) {
            var proxy = ActiveProxies.FirstOrDefault(x => x.Id == id);
            if (proxy != null) {
                proxy.Interceptor.ShutdownAsync();
                ActiveProxies.Remove(proxy);
                Log.Information("已关闭代理 {Nickname} ({Id})", proxy.GetNickName(), proxy.Id);
                return;
            }

            var proxy1 = ActiveProxies1.FirstOrDefault(x => x.Id == id);
            if (proxy1 == null) return;
            proxy1.Shutdown();
            ActiveProxies1.Remove(proxy1);
            Log.Information("已关闭代理 {Nickname} ({Id})", proxy1.GetNickName(), proxy1.Id);
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
                Log.Information("已关闭代理 {Nickname} ({Id})", proxy.GetNickName(), proxy.Id);
            }
        }
    }

    // 关闭白端游戏
    public static void CloseGame(string id)
    {
        lock (SafeLock) {
            var launcher = GetLauncherService(id);
            if (launcher == null) return;
            ActiveLaunchers.Remove(launcher);
            launcher.ShutdownAsync();
            Log.Information("白端游戏 {id} 已关闭", launcher.GetPid());
        }
    }

    // 获取白端游戏
    private static LauncherService? GetLauncherService(string id)
    {
        var index = 0;
        foreach (var launcher in ActiveLaunchers) {
            if (index.ToString().Equals(id)) return launcher;
            index++;
        }

        return null;
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

    private static int GetProxiesIndex()
    {
        lock (SafeLock) {
            var max = ActiveProxies.Select(proxy => proxy.Id).Prepend(0).Max();
            max = ActiveProxies1.Select(proxy => proxy.Id).Prepend(max).Max();
            return max + 1;
        }
    }

    // 清理过期游戏白端
    private static void DisposeActive()
    {
        lock (SafeLock) {
            foreach (var launcher in ActiveLaunchers.ToList().Where(launcher => !launcher.IsRunning())) {
                Log.Information("白端游戏 {id} 已清理", launcher.GetPid());
                ActiveLaunchers.Remove(launcher);
                launcher.ShutdownAsync().Wait();
            }

            foreach (var launcher in ActiveProxies1.ToList().Where(launcher => !launcher.IsRunning())) {
                Log.Information("游戏代理 {id} 已清理", launcher.GetRunningPid());
                ActiveProxies1.Remove(launcher);
                launcher.Shutdown();
            }
        }
    }
}