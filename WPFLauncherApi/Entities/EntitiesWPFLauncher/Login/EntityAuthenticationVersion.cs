using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityAuthenticationVersion {
    [JsonPropertyName("version")] public required string Version { get; set; }


    [JsonPropertyName("launcher_md5")] public string LauncherMd5 { get; set; } = string.Empty;


    [JsonPropertyName("updater_md5")] public string UpdaterMd5 { get; set; } = string.Empty;
}