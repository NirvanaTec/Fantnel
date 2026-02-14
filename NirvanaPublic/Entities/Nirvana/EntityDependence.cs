using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityDependence {
    [JsonPropertyName("mode")]
    public required string Mode { get; set; }

    [JsonPropertyName("data")]
    public required EntityDependence2[] Data { get; set; }
}