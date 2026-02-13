using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher;

public class InterConnGameStart {
    [JsonPropertyName("game_id")] public required string GameId { get; set; }


    [JsonPropertyName("game_type")] public string GameType { get; set; } = "2";


    [JsonPropertyName("strict_mode")] public bool StrictMode { get; set; } = true;


    [JsonPropertyName("item_list")] public required string[] ItemList { get; set; }
}