using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntityQuerySearchByGameResponse {
    [JsonPropertyName("mc_version_id")]
    public required int McVersionId { get; set; }

    [JsonPropertyName("game_type")]
    public required int GameType { get; set; }

    [JsonPropertyName("iid_list")]
    public required List<ulong>? IidList { get; set; }
}