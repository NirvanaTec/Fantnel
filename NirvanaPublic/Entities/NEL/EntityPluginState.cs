using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.NEL;

public class EntityPluginState
{
    public string[]? Dependencies; // 依赖插件ID [真实ID]
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("version")] public string? Version { get; set; }

    [JsonPropertyName("author")] public string? Author { get; set; }

    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("path")] public string? Path { get; set; }
}