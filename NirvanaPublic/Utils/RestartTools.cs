using NirvanaPublic.Message;

namespace NirvanaPublic.Utils;

public static class RestartTools
{
    /**
     * 根据参数自动执行
     * @param args 参数
     * @return 是否开启web服务
     * @param logoInit 日志初始化
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
        while (true)
        {
            Thread.Sleep(9000);
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