using System.Text.Json.Serialization;
using NirvanaAPI.Utils.CodeTools;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;

namespace NirvanaPublic.Entities.NEL;

public class EntityServerDetail {
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("gameVersion")]
    public string? GameVersion { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("fullDescription")]
    public string? FullDescription { get; set; }

    [JsonPropertyName("brief_image_urls")]
    public string[]? BriefImageUrls { get; set; }

    public void Set(EntityNetGameItem? server)
    {
        if (server == null) throw new ErrorCodeException(ErrorCode.IdError);
        Id = server.EntityId;
        Name = server.Name;
    }

    public void Set(EntityQueryNetGameDetailItem data)
    {
        // 成功检测
        if (data == null) throw new ErrorCodeException(ErrorCode.LogInNot);
        Author = data.DeveloperName;
        // unix 时间戳 转换为 文本
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(data.PublishTime).ToString("yyyy-MM-dd");
        GameVersion = "";
        foreach (var version in data.McVersionList) GameVersion += version.Name + ", ";
        // 删除最后一个逗号
        GameVersion = GameVersion.TrimEnd(',', ' ');
        FullDescription = data.DetailDescription;
        BriefImageUrls = data.BriefImageUrls;
    }

    public void Set(EntityNetGameServerAddress data)
    {
        if (data == null) throw new ErrorCodeException(ErrorCode.AddressError);
        Address = data.Host;
        if (data.Port != 25565) Address += $":{data.Port}";
    }
}