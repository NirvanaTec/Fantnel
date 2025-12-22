using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Plugin;

public class EntityPluginDependency
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }
}