using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityMode {
    [JsonPropertyName("mode")]
    public required string Mode { get; init; }
}