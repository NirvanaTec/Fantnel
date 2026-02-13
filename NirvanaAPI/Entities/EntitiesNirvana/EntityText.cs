using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityText {
    [JsonPropertyName("text")] public required string Text { get; init; }
}