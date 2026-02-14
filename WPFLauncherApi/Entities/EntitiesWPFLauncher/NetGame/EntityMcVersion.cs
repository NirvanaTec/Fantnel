using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityMcVersion {
    [JsonPropertyName("mcversionid")]
    public int McVersionId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}