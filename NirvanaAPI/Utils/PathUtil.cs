using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace NirvanaAPI.Utils;

public static class PathUtil {
    public static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".game_cache");

    public static readonly string ResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

    public static readonly string UpdaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater");

    // 脚本后缀
    public static readonly string ScriptSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".bat" : ".sh";

    public static readonly string ScriptPath = Path.Combine(UpdaterPath, "update" + ScriptSuffix);

    public static readonly string CustomModsPath = Path.Combine(ResourcePath, "mods");

    public static readonly string GamePath = Path.Combine(CachePath, "Game");

    public static readonly string GameBasePath = Path.Combine(GamePath, "Base");

    public static readonly string GameModsPath = Path.Combine(CachePath, "GameMods");

    public static readonly string CppGamePath = Path.Combine(CachePath, "CppGame");

    // 分割路径
    public static readonly string PathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";

    public static string JavaExePath => GetJavaExePath(); // javaw.exe

    // public static readonly string WebSitePath = Path.Combine(ResourcePath, "static");

    public static string JavaPath => GetJavaPath();

    public static string Jre8Path => Path.Combine(JavaPath, "jre8");

    public static string Jre17Path => Path.Combine(JavaPath, "jdk17");

    /**
     * 获取Java路径
     * 先检测 网易版，不存在返回默认
     * 网易版 : 先检测注册表，再手动检测 C:/MCLDownload
     */
    private static string GetJavaPath()
    {
        var cacheJava = Path.Combine(CachePath, "Java"); // .game_cache/Java
        // 非 win 版 或 已存在 .game_cache/Java
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || ExistJava(cacheJava))
            return cacheJava;
        var reg4399 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\PC4399_MCLauncher");
        var reg163 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Netease\MCLauncher");
        if (reg4399?.GetValue("DownloadPath") is string patch) {
            var javaPath = Path.Combine(patch, "ext", "jre-v64-220420");
            if (ExistJava(javaPath)) return javaPath;
        } else if (reg163?.GetValue("DownloadPath") is string patch2) {
            var javaPath = Path.Combine(patch2, "ext", "jre-v64-220420");
            if (ExistJava(javaPath)) return javaPath;
        } else {
            var wpf = Path.Combine("C:/MCLDownload", "ext", "jre-v64-220420");
            if (ExistJava(wpf)) return wpf;
            wpf = Path.Combine("D:/MCLDownload", "ext", "jre-v64-220420");
            if (ExistJava(wpf)) return wpf;
        }

        return cacheJava;
    }

    private static string GetJavaExePath()
    {
        // Mac / Linux 没有 javaw.exe
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javaw.exe" : "java";
    }

    // javaw 是否存在
    public static bool ExistJava(string path)
    {
        return File.Exists(Path.Combine(path, "jdk17", "bin", JavaExePath)) &&
               File.Exists(Path.Combine(path, "jre8", "bin", JavaExePath));
    }

    // public static void OpenDirectory(string path)
    // {
    //     try
    //     {
    //         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //             Process.Start("explorer.exe", path);
    //         else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    //             Process.Start("open", path);
    //         else
    //             Process.Start("xdg-open", path);
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error(ex, "Failed to open directory: {Path}", path);
    //     }
    // }
}