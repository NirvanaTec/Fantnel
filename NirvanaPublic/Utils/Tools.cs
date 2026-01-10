using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Utils;

public static class Tools
{
    public static (T[], string) GetValueOrDefault<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "resources", fileName);
        List<T>? entity = [];
        if (!File.Exists(path)) return (entity.ToArray(), path);

        var json = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            // 异常格式处理
            entity = JsonSerializer.Deserialize<List<T>>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        // 处理空数组
        entity ??= [];
        // 过滤空值
        foreach (var item in entity.ToArray())
            if (item == null)
                entity.Remove(item);

        return (entity.ToArray(), path);
    }

    // 获取异常信息 【简化版】
    public static string GetMessage(Exception exception)
    {
        if (exception is AggregateException aggregateException)
        {
            var message = aggregateException.InnerExceptions.Aggregate("",
                (current, innerException) => current + GetMessage(innerException) + ", ");
            return message.TrimEnd(',', ' ');
        }

        if (exception is not ErrorCodeException errorCodeException) return exception.Message;
        {
            var message = errorCodeException.Entity.Msg;
            if (message != null) return message;
        }
        return exception.Message;
    }

    /**
     * 计算文件的MD5哈希值
     * @param filePath 文件绝对路径
     * @return 32位小写MD5字符串
     */
    public static string GetFileMd5(string filePath)
    {
        // 使用using语句确保资源释放
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        // 计算哈希值并转换为十六进制字符串
        var hashBytes = md5.ComputeHash(stream);
        // 去除分隔符并转为小写（符合通用MD5格式）
        return Convert.ToHexStringLower(hashBytes);
    }

    /**
     * 获取中间文本
     */
    public static string GetBetweenStrings(string source, string startString, string endString)
    {
        var startIndex = source.IndexOf(startString, StringComparison.Ordinal);
        if (startIndex == -1)
            return string.Empty;

        startIndex += startString.Length;

        var endIndex = source.IndexOf(endString, startIndex, StringComparison.Ordinal);
        return endIndex == -1 ? string.Empty : source.Substring(startIndex, endIndex - startIndex);
    }

    /**
     * 同步计算文件的SHA256哈希值
     * @param filePath 文件路径
     * @return 文件的SHA256哈希值（小写十六进制字符串）
     */
    public static string ComputeSha256(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("文件路径不能为空", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"文件不存在: {filePath}");

        using var sha256 = SHA256.Create();
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var hashBytes = sha256.ComputeHash(fileStream);
        return Convert.ToHexStringLower(hashBytes);
    }

    /**
     * 检查指定端口是否正在被使用
     * @param port 要检查的端口号
     * @return 如果端口正在被使用则返回true，否则返回false
     */
    private static bool IsPortInUse(int port)
    {
        // 获取本机的网络属性信息
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        // 获取所有正在监听的TCP端点（包含端口号）
        var tcpEndPoints = ipGlobalProperties.GetActiveTcpListeners();

        // 遍历检查目标端口是否存在
        return tcpEndPoints.Any(endPoint => endPoint.Port == port);
    }

    /**
     * 获取未被占用的端口
     * @param startPort 起始端口号
     * @return 未被占用的端口号，如果所有端口都被占用则返回-1
     */
    public static int GetUnusedPort(int startPort)
    {
        for (var port = startPort; port <= startPort + 1024; port++)
            if (!IsPortInUse(port))
                return port;
        return -1;
    }

    // 获取IP地址
    public static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        return "localhost";
    }

    /**
     * 检测当前操作系统并返回对应的模式
     * @return win | linux | mac
     */
    public static string DetectOperatingSystemMode()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "mac" : "win";
    }

    public static bool IsReleaseVersion()
    {
#if DEBUG
        return false;
#else
            return true;
#endif
    }

    /**
     * 检测当前架构并返回对应的模式
     * @return arm64 | x64
     */
    public static string DetectArchitectureMode()
    {
        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";
    }
    
}