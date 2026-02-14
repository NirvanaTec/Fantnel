using NirvanaAPI.Utils;

namespace NirvanaPublic;

public static class PublicProgram {
    // Fantnel 版本
    public const string Version = "1.5.0";
    public const int VersionId = 5;

    // 是最新版本
#pragma warning disable CA2211
    public static bool LatestVersion = true;
#pragma warning restore CA2211

    // 检查更新的模式 win | linux | mac
    public static readonly string Mode = Tools.DetectOperatingSystemMode();

    // arm64 | x64
    public static readonly string Arch = Tools.DetectArchitectureMode();

    // 是否是发布版本
    public static readonly bool Release = Tools.IsReleaseVersion();
}