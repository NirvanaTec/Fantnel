using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;

public class EntityMcDownloadVersion {
    [JsonPropertyName("mc_version")] public required uint McVersion { get; set; }
}