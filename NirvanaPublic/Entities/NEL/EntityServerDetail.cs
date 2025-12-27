using System.Text.Json.Serialization;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using NirvanaPublic.Utils;
using NirvanaPublic.Utils.ViewLogger;

namespace NirvanaPublic.Entities.NEL;

public class EntityServerDetail
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("author")] public string? Author { get; set; }

    [JsonPropertyName("createdAt")] public string? CreatedAt { get; set; }

    [JsonPropertyName("gameVersion")] public string? GameVersion { get; set; }

    [JsonPropertyName("address")] public string? Address { get; set; }

    [JsonPropertyName("fullDescription")] public string? FullDescription { get; set; }

    [JsonPropertyName("brief_image_urls")] public string[]? BriefImageUrls { get; set; }

    public void Set(EntityNetGameItem? server)
    {
        if (server == null) throw new Code.ErrorCodeException(Code.ErrorCode.IdError);
        Id = server.EntityId;
        Name = server.Name;
    }

    public void Set(Entity<EntityQueryNetGameDetailItem> server)
    {
        // 成功检测
        Tools.EntitySafe(server);
        var data = server.Data;
        if (data == null) throw new Code.ErrorCodeException(Code.ErrorCode.LogInNot);
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

    public void Set(Entity<EntityNetGameServerAddress> server)
    {
        Tools.EntitySafe(server);
        if (server.Data == null) throw new Code.ErrorCodeException(Code.ErrorCode.AddressError);
        Address = server.Data.Ip;
        if (server.Data.Port != 25565) Address += $":{server.Data.Port}";
    }
}