using System.Text.Json.Serialization;

namespace Nirvana.Chat.Entities;

public class EntityChat {
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "chat";

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}