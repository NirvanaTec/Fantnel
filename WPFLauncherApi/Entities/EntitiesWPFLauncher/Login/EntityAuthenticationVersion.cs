using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityAuthenticationVersion
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("version")] public required string Version { get; set; }

    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("launcher_md5")] public string LauncherMd5 { get; set; } = string.Empty;

    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("updater_md5")] public string UpdaterMd5 { get; set; } = string.Empty;
}