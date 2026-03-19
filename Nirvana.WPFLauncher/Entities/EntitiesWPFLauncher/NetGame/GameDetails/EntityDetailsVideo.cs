using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameDetails;

public class EntityDetailsVideo {
    [JsonPropertyName("cover")]
    public string Cover { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}