using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Entities.NEL;

public class EntitySkinDetail
{
    [JsonPropertyName("entity_id")] public string? EntityId { get; set; }

    [JsonPropertyName("brief_summary")] public string? BriefSummary { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("title_image_url")] public string? TitleImageUrl { get; set; }

    [JsonPropertyName("like_num")] public int? LikeNum { get; set; }

    [JsonPropertyName("developer_name")] public string? DeveloperName { get; set; }
    [JsonPropertyName("publish_time")] public string? PublishTime { get; set; }
    [JsonPropertyName("download_num")] public long? DownloadNum { get; set; }

    public void Set(EntityQueryNetSkinItem? skinItem)
    {
        if (skinItem == null) throw new ErrorCodeException(ErrorCode.IdError);
        DeveloperName = skinItem.DeveloperName;
        // unix 时间戳 转换为 文本
        PublishTime = DateTimeOffset.FromUnixTimeSeconds(skinItem.PublishTime).ToString("yyyy-MM-dd");
        DownloadNum = skinItem.DownloadNum;
    }

    public void Set(EntitySkin? skinDetails)
    {
        if (skinDetails == null) throw new ErrorCodeException(ErrorCode.IdError);
        EntityId = skinDetails.EntityId;
        BriefSummary = skinDetails.BriefSummary;
        Name = skinDetails.Name;
        TitleImageUrl = skinDetails.TitleImageUrl;
        LikeNum = skinDetails.LikeNum;
    }
}