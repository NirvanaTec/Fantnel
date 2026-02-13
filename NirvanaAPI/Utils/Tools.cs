using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NirvanaAPI.Utils.CodeTools;
using Serilog;

namespace NirvanaAPI.Utils;

public static class Tools {
    private static bool _isDebugMode;

    [Conditional("DEBUG")]
    private static void SetDebugMode()
    {
        _isDebugMode = true;
    }

    public static bool IsReleaseVersion()
    {
        SetDebugMode();
        return !_isDebugMode;
    }

    public static (T[], string) GetValueOrDefaultList<T>(string fileName)
    {
        var (list, path) = GetValueOrDefault<List<T?>>(fileName);

        // 处理空数组
        list ??= [];

        // 过滤空值
        var listNotNull = list.OfType<T>().ToArray();

        return (listNotNull, path);
    }


    public static (T?, string) GetValueOrDefault<T>(string fileName)
    {
        var path = Path.Combine(PathUtil.ResourcePath, fileName);

        if (!File.Exists(path)) {
            return (default, path);
        }

        try {
            // 异常格式处理
            var json = File.ReadAllText(path, Encoding.UTF8);
            return (JsonSerializer.Deserialize<T>(json), path);
        } catch (Exception e) {
            Log.Error("读取文件 {Path} 异常: {Message}", path, e.Message);
        }

        return (default, path);
    }

    // 获取异常信息 【简化版】
    public static string GetMessage(Exception exception)
    {
        if (exception is AggregateException aggregateException) {
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
    public static string GetLocalIpAddress(bool localhost = true)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        return localhost ? "localhost" : "127.0.0.1";
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

    /**
     * 检测当前架构并返回对应的模式
     * @return arm64 | x64
     */
    public static string DetectArchitectureMode()
    {
        return RuntimeInformation.ProcessArchitecture switch {
            Architecture.Arm64 or Architecture.Arm or Architecture.Armv6 => "arm64",
            _ => "x64"
        };
    }

    /**
     * 重启当前进程
     * @param isExit 退出当前进程
     * @param arguments 附加参数
     */
    public static Process? Restart(bool isExit = true, List<string>? arguments = null)
    {
        var fileName = GetProcessLocation();
        var arg = GetProcessArguments(arguments);
        var startInfo = new ProcessStartInfo {
            FileName = fileName,
            Arguments = arg,
            UseShellExecute = true
        };
        Log.Information("正在重启: {ExecutablePath} {Arguments}", fileName, arg);
        var process = Process.Start(startInfo);
        if (isExit) Environment.Exit(0);
        return process;
    }

    /**
     * 前/尾 不包含空格
     * @return 当前进程的附加参数
     */
    public static string GetProcessArguments(List<string>? arguments = null)
    {
        var arg = Environment.GetCommandLineArgs().Aggregate("", (current, lineArg) => current + lineArg + " ");
        if (arguments != null) arg = arguments.Aggregate(arg, (current, argument) => current + argument + " ");
        // 移除最后一个空格
        // "a " > "a"
        return arg.Length >= 2 ? arg[..^1] : arg;
    }

    /**
     * @return 当前进程的路径
     */
    public static string GetProcessLocation()
    {
        var currentProcess = Process.GetCurrentProcess();
        var mainModule = currentProcess.MainModule;
        if (mainModule != null) {
            var fileName1 = mainModule.FileName;
            if (!string.IsNullOrEmpty(fileName1)) return fileName1;
        }

        var fileName2 = Environment.ProcessPath;
        return string.IsNullOrEmpty(fileName2) ? throw new Exception("无法确定自身运行路径，无法重启当前进程。") : fileName2;
    }

    /**
    * 获取中间文本
    */
    public static string GetBetweenStrings(string source, string startString, string endString)
    {
        var startIndex = source.IndexOf(startString, StringComparison.Ordinal);
        if (startIndex == -1) {
            return string.Empty;
        }

        startIndex += startString.Length;

        var endIndex = source.IndexOf(endString, startIndex, StringComparison.Ordinal);
        return endIndex == -1 ? string.Empty : source.Substring(startIndex, endIndex - startIndex);
    }

    // 保存Shell脚本
    public static async Task SaveShellScript(string filePath, string content)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            await File.WriteAllTextAsync(filePath, content, Encoding.GetEncoding(936)); // GBK 编码
        } else {
            await File.WriteAllTextAsync(filePath, content);
            // 设置权限
            Log.Information("设置权限: {filePath}", filePath);
            FileUtil.SetUnixFilePermissions(filePath);
        }
    }
}