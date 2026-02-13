using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityNirvanaLogin {
    [JsonPropertyName("online")] public string? Token { get; set; }

    [JsonPropertyName("msg")] public required string Msg { get; set; }
}