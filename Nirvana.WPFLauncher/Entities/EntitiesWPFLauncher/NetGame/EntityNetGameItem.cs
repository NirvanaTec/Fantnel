using System.Text.Json.Serialization;
using Nirvana.WPFLauncher.Entities.EntitiesConverter;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;

public class EntityNetGameItem {
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("brief_summary")]
    public string BriefSummary { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("online_count")]
    [JsonConverter(typeof(NetEaseIntConverter))]
    public string OnlineCount { get; set; } = string.Empty;
    
    [JsonPropertyName("title_image_url")] // 来自详细信息
    public string TitleImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("version")] // 来自详细信息
    public string Version { get; set; } = string.Empty;

    public bool TitleImageSafe()
    {
        return TitleImageUrl.Contains("http") || TitleImageUrl.Contains("/image/");
    }
}