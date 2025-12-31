using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameCharacters;

public class EntityGameCharacter
{
    [JsonPropertyName("game_id")] public string GameId { get; set; } = string.Empty;

    [JsonPropertyName("game_type")] public int GameType { get; set; } = 2;

    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("create_time")] public int CreateTime { get; set; } = 555555;

    [JsonPropertyName("expire_time")] public int ExpireTime { get; set; }
}