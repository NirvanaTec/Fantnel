using System.Text.Json.Serialization;

namespace NirvanaChat.Entities;

public class EntityChat {
    [JsonPropertyName("mode")] public string Mode { get; set; } = "chat";

    [JsonPropertyName("message")] public required string Message { get; set; }
}