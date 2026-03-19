using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;

public class EntityMcVersion {
    [JsonPropertyName("mcversionid")]
    public int McVersionId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}