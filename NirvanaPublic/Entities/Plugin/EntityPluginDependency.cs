using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Plugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityPluginDependency
{
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("id")] public required string Id { get; set; }

    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("name")] public required string Name { get; set; }
}