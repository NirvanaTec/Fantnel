using NirvanaAPI.Utils;

namespace Nirvana.Public;

public static class PublicProgram {
    // Fantnel 版本
    public const string Version = "1.6.1";
    public const int VersionId = 7;

    // 是最新版本
    public static bool LatestVersion = true;

    // 检查更新的模式 win | linux | mac
    public static readonly string Mode = Tools.DetectOperatingSystemMode();

    // arm64 | x64
    public static readonly string Arch = Tools.DetectArchitectureMode();

    // 是否是发布版本
    public static readonly bool Release = Tools.IsReleaseVersion();
}