using System.Diagnostics;
using NirvanaPublic.Message;
using Serilog;

namespace NirvanaPublic.Utils;

public static class RestartTools
{
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
        if (mode != "proxy") return true;
        var id = Get("id", args);
        var name = Get("name", args);
        var port = Get("port", args);
        if (string.IsNullOrEmpty(port)) port = "25565";
        InitProgram.NelInit1();
        ProxiesMessage.StartProxyAsync1(id, name, int.Parse(port)).Wait();
        Maintenance(args);
        return false;
    }

    /**
     * 防止 线程 因 执行完成 导致程序退出
     */
    private static void Maintenance(string[] args)
    {
        var pidString = Get("MainPid", args);
        var pid = -1;
        if (!string.IsNullOrEmpty(pidString))
        {
            pid = int.Parse(pidString);
        }
        while (true)
        {
            if (pid == -1)
            {
                Thread.Sleep(9000);
                continue;
            }
            Thread.Sleep(1000);
            try
            {
                Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
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

    /**
     * 获取参数值
     */
    private static string Get(string name, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
            if (args[i] == "--" + name)
                return args[i + 1];

        return "";
    }
}