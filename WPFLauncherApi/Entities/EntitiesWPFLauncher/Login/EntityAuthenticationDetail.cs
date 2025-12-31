using System.Text.Json.Serialization;
using WPFLauncherApi.Utils;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityAuthenticationDetail
{
    [JsonPropertyName("os_name")] public string OsName { get; set; } = "windows";

    [JsonPropertyName("os_ver")] public string OsVer { get; set; } = "Microsoft Windows 11 专业版";

    [JsonPropertyName("mac_addr")] public string MacAddr { get; set; } = StringGenerator.GenerateRandomMacAddress("");

    [JsonPropertyName("udid")] public required string Udid { get; set; }

    [JsonPropertyName("app_ver")] public required string AppVersion { get; set; }

    [JsonPropertyName("sdk_ver")] public string SdkVersion { get; set; } = string.Empty;

    [JsonPropertyName("network")] public string Network { get; set; } = string.Empty;

    [JsonPropertyName("disk")] public required string Disk { get; set; }

    [JsonPropertyName("is64bit")] public string Is64Bit { get; set; } = "1";

    [JsonPropertyName("video_card1")] public string VideoCard1 { get; set; } = "Microsoft Hyper-V 视频";

    [JsonPropertyName("video_card2")] public string VideoCard2 { get; set; } = "Microsoft Remote Display Adapter";

    [JsonPropertyName("video_card3")] public string VideoCard3 { get; set; } = string.Empty;

    [JsonPropertyName("video_card4")] public string VideoCard4 { get; set; } = string.Empty;

    [JsonPropertyName("launcher_type")] public string LauncherType { get; set; } = "PC_java";

    [JsonPropertyName("pay_channel")] public required string PayChannel { get; set; }

    [JsonPropertyName("dotnet_ver")] public string DotnetVersion { get; set; } = "4.8.0";

    [JsonPropertyName("cpu_type")] public string CpuType { get; set; } = "Intel(R) Core(TM) i9-14900KF";

    [JsonPropertyName("ram_size")] public string RamSize { get; set; } = "8589934592";

    [JsonPropertyName("device_width")] public string DeviceWidth { get; set; } = "1920";

    [JsonPropertyName("device_height")] public string DeviceHeight { get; set; } = "1080";

    [JsonPropertyName("os_detail")] public string OsDetail { get; set; } = "10.0.26100";
}