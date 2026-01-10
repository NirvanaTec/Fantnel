using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntityComponentDownloadInfoResponseSub
{
    [JsonPropertyName("java_version")] public required int JavaVersion { get; set; }

    [JsonPropertyName("mc_version_name")] public required string McVersionName { get; set; }

    [JsonPropertyName("res_url")] public required string ResUrl { get; set; }

    [JsonPropertyName("res_size")] public required long ResSize { get; set; }

    [JsonPropertyName("res_md5")] public required string ResMd5 { get; set; }

    [JsonPropertyName("jar_md5")] public required string JarMd5 { get; set; }

    [JsonPropertyName("res_name")] public required string ResName { get; set; }

    [JsonPropertyName("res_version")] public required int ResVersion { get; set; }
}