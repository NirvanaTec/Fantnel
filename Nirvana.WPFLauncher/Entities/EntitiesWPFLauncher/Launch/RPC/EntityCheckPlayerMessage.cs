using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Launch.RPC;

public class EntityCheckPlayerMessage {
    [JsonPropertyName("a")]
    public required int Length { get; set; }

    [JsonPropertyName("b")]
    public required string Message { get; set; }
}