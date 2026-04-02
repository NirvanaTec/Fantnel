using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityAccountNirvanaConfig {
    [JsonPropertyName("account")]
    public required string Account { get; set; }

    [JsonPropertyName("days")]
    public required double Days { get; set; }

    [JsonPropertyName("hideAccount")]
    public required bool HideAccount { get; set; }
}