using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntityQuerySearchByGameRequest {
    [JsonPropertyName("mc_version_id")]
    public required int McVersionId { get; set; }

    [JsonPropertyName("game_type")]
    public required int GameType { get; set; }
}