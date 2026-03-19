using System.Text.Json.Serialization;
using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameSkin;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;

namespace Nirvana.Public.Entities.NEL;

public class EntitySkinDetail {
    public EntitySkinDetail(string id)
    {
        Set(NPFLauncher.GetSkinDetailsAsync(id).Result);
    }

    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("brief_summary")]
    public string? BriefSummary { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title_image_url")]
    public string? TitleImageUrl { get; set; }

    [JsonPropertyName("like_num")]
    public int? LikeNum { get; set; }

    [JsonPropertyName("developer_name")]
    public string? DeveloperName { get; set; }

    [JsonPropertyName("publish_time")]
    public string? PublishTime { get; set; }

    [JsonPropertyName("download_num")]
    public long? DownloadNum { get; set; }

    private void Set(EntityQueryNetSkinItem? item)
    {
        if (item == null) throw new ErrorCodeException(ErrorCode.IdError);
        CacheManager.ClearCacheImage(item);
        DeveloperName = item.DeveloperName;
        // // unix 时间戳 转换为 文本
        PublishTime = DateTimeOffset.FromUnixTimeSeconds(item.PublishTime).ToString("yyyy-MM-dd");
        DownloadNum = item.DownloadNum;
        EntityId = item.EntityId;
        BriefSummary = item.BriefSummary;
        Name = item.Name;
        TitleImageUrl = item.TitleImageUrl;
        LikeNum = item.LikeNum;
    }
}