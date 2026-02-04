using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher;

public class InterConnGameStart {
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("game_id")] public required string GameId { get; set; }

    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("game_type")] public string GameType { get; set; } = "2";

    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("strict_mode")] public bool StrictMode { get; set; } = true;

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("item_list")] public required string[] ItemList { get; set; }
}