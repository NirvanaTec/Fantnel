using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Launch;

public class EntityUserPropertiesEx {
    [JsonPropertyName("GameType")] public required int GameType { get; set; }

    [JsonPropertyName("isFilter")] public required bool IsFilter { get; set; }

    [JsonPropertyName("channel")] public required string Channel { get; set; }

    [JsonPropertyName("timedelta")] public required int TimeDelta { get; set; }

    [JsonPropertyName("launcherVersion")] public required string LauncherVersion { get; set; }
}