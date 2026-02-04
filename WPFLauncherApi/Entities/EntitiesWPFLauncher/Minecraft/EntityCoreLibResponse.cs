using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;

public class EntityCoreLibResponse {
    [JsonPropertyName("core_lib_md5")] public required string CoreLibMd5 { get; set; }

    [JsonPropertyName("core_lib_name")] public required string CoreLibName { get; set; }

    [JsonPropertyName("core_lib_size")] public required int CoreLibSize { get; set; }

    [JsonPropertyName("core_lib_url")] public required string CoreLibUrl { get; set; }

    [JsonPropertyName("mc_version")] public required int McVersion { get; set; }

    [JsonPropertyName("md5")] public required string Md5 { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("refresh_time")] public required int RefreshTime { get; set; }

    [JsonPropertyName("size")] public required int Size { get; set; }

    [JsonPropertyName("url")] public required string Url { get; set; }

    [JsonPropertyName("version")] public required string Version { get; set; }
}