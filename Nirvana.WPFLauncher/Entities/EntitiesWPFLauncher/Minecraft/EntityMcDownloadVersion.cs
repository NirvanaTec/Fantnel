using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Minecraft;

public class EntityMcDownloadVersion {
    [JsonPropertyName("mc_version")]
    public required uint McVersion { get; set; }
}