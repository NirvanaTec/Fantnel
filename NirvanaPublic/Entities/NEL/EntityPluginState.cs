using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.NEL;

public class EntityPluginState {
    public string[]? Dependencies; // 依赖插件ID [真实ID]

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("author")]
    public required string Author { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("path")]
    public required string Path { get; set; }
}