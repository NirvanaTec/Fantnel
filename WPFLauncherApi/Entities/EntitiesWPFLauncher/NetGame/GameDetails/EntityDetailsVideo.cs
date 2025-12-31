using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityDetailsVideo
{
    [JsonPropertyName("cover")] public string Cover { get; set; } = string.Empty;

    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
}