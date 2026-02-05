using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityDependence2 {
    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("id")] public required string Id { get; set; }
}