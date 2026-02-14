using System.Diagnostics;
using NirvanaPublic.Message;
using Serilog;

namespace NirvanaPublic.Utils;

public static class RestartTools {
    /**
     * 根据参数自动执行
     * @param args 参数
     * @param logoInit 日志初始化
     * @return 是否开启web服务
     */
    public static bool Main(string[] args, Action logoInit)
    {
        // 初始化日志
        logoInit.Invoke();

        var mode = Get("mode", args);
        if (mode == "proxy") {
            var id = Get("id", args, null);
            var name = Get("name", args, null);
            var accountId = Get<int>("account", args);
            var port = Get("port", args, 25565);
            var proxyMode = Get("proxyMode", args, "net");
            AccountMessage.DisableDefaultLogin(); // 禁止默认登录
            AccountMessage.SwitchAccountToForce(accountId); // 强制切换账号
            InitProgram.NelInit1();
            ProxiesMessage.StartProxyAsyncTo(id, name, port, proxyMode).Wait();
            Maintenance(args);
        }
        return true;
    }

    /**
     * 防止 线程 因 执行完成 导致程序退出
     */
    private static void Maintenance(string[] args)
    {
        var pidString = Get("MainPid", args);
        var pid = -1;
        if (!string.IsNullOrEmpty(pidString)) {
            pid = int.Parse(pidString);
        }

        while (true) {
            if (pid == -1) {
                Thread.Sleep(9000);
                continue;
            }

            Thread.Sleep(1000);
            try {
                Process.GetProcessById(pid);
            } catch (ArgumentException) {
                Log.Error("主进程 {pid} 已退出", pid);
                Log.Error("将于 3秒 后退出！");
                Thread.Sleep(1000);
                Log.Error("将于 2秒 后退出！");
                Thread.Sleep(1000);
                Log.Error("将于 1秒 后退出！");
                Thread.Sleep(2000);
                break;
            }
        }
    }

    private static string Get(string name, string[] args, string? defaultValue = "")
    {
        return Get<string>(name, args, defaultValue);
    }

    private static T Get<T>(string name, string[] args, T? defaultValue = default)
    {
        for (var i = 0; i < args.Length; i++){
            if (args[i] == "--" + name) {
                return (T) Convert.ChangeType(args[i + 1], typeof(T));
            }
        }
        return defaultValue ?? throw new Exception($"参数 {name} 不能为空");
    }
    
}