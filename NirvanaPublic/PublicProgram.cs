using NirvanaPublic.Utils;

namespace NirvanaPublic;

public static class PublicProgram
{
    // Fantnel 版本
    public const string Version = "1.2.0";
    public const int VersionId = 2;

    // 是最新版本
#pragma warning disable CA2211
    public static bool LatestVersion = true;
#pragma warning restore CA2211

    // 检查更新的模式 win64G | linux64 | mac64
    public static readonly string Mode = Tools.DetectOperatingSystemMode();

    // 是否是发布版本
    public static readonly bool Release = Tools.IsReleaseVersion();
}