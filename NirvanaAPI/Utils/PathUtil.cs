using System.Runtime.InteropServices;

namespace NirvanaAPI.Utils;

public static class PathUtil {
    // 系统架构 - win.x64
    public static readonly string DetectOperating = Tools.DetectOperatingSystemMode();
    public static readonly string SystemArch = DetectOperating + "." + Tools.DetectArchitectureMode();

    public static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".game_cache");
    public static readonly string ResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

    public static readonly string UpdaterBasePath = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string UpdaterPath = Path.Combine(UpdaterBasePath, "updater");

    // 脚本后缀
    public static readonly string ScriptSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".bat" : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".command" : ".sh";

    public static readonly string ScriptPath = Path.Combine(UpdaterPath, "update" + ScriptSuffix);

    public static readonly string CustomModsPath = Path.Combine(ResourcePath, "mods");

    public static readonly string GamePath = Path.Combine(CachePath, "Game");

    public static readonly string GameBasePath = Path.Combine(GamePath, "Base");

    public static readonly string GameModsPath = Path.Combine(CachePath, "GameMods");

    public static readonly string CppGamePath = Path.Combine(CachePath, "CppGame");

    // 分割路径
    public static readonly string PathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";

    public static readonly string WebSitePath = Path.Combine(ResourcePath, "static");

    public static readonly string CacheImagePath = Path.Combine(WebSitePath, "image");

    public static readonly string JavaPath = Path.Combine(CachePath, "Java");

    public static readonly string Jre8Path = Path.Combine(JavaPath, "jre8");

    public static readonly string Jre17Path = Path.Combine(JavaPath, "jdk17");

    public static string JavaExePath => GetJavaExePath(); // javaw.exe

    private static string GetJavaExePath()
    {
        // Mac / Linux 没有 javaw.exe
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return NirvanaConfig.GetBool("useJavaW") ? "javaw.exe" : "java.exe";
        }

        return "java";
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