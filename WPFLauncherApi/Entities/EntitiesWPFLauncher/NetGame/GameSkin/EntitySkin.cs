using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;

public class EntitySkin
{
    [JsonPropertyName("entity_id")] public required string EntityId { get; set; }

    [JsonPropertyName("brief_summary")] public required string BriefSummary { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("title_image_url")] public required string TitleImageUrl { get; set; }

    [JsonPropertyName("like_num")] public required int LikeNum { get; set; }
}