using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityQueryNetSkinItem {
    [JsonPropertyName("entity_id")]
    public required string EntityId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("developer_name")]
    public required string DeveloperName { get; set; }

    [JsonPropertyName("brief_summary")]
    public required string BriefSummary { get; set; }

    [JsonPropertyName("publish_time")]
    public required long PublishTime { get; set; }

    [JsonPropertyName("download_num")]
    public required long DownloadNum { get; set; }

    [JsonPropertyName("like_num")]
    public required int LikeNum { get; set; }

    [JsonPropertyName("title_image_url")]
    public string TitleImageUrl { get; set; } = string.Empty;

    public bool TitleImageSafe()
    {
        return TitleImageUrl.Contains("http");
    }
}