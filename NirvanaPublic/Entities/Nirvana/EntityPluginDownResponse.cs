using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityPluginDownResponse
{
    [JsonPropertyName("fileHash")] public string? FileHash { get; set; }

    [JsonPropertyName("fileSize")] public long? FileSize { get; set; }

    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("dependencies")] public EntityPluginDownResponse[]? Dependencies { get; set; }
}