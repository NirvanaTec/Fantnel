using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityValue {
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}