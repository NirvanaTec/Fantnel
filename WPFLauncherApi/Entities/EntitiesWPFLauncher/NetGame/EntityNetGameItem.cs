using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesConverter;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;

// ReSharper disable once ClassNeverInstantiated.Global
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

    [JsonPropertyName("title_image_url")]
    public string TitleImageUrl { get; set; } = string.Empty;
}