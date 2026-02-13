using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityNirvanaInfo {
    [JsonPropertyName("days")] public required double Days { get; set; }

    [JsonPropertyName("msg")] public required string Msg { get; set; }
}