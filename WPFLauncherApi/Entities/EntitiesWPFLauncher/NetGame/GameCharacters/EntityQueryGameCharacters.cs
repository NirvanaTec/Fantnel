using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;

public class EntityQueryGameCharacters {
    [JsonPropertyName("offset")] public int Offset { get; set; }

    [JsonPropertyName("length")] public int Length { get; set; } = 10;

    [JsonPropertyName("user_id")] public required string UserId { get; set; }

    [JsonPropertyName("game_id")] public required string GameId { get; set; }

    [JsonPropertyName("game_type")] public string GameType { get; set; } = "2";
}