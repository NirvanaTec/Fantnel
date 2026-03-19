using System.Text.Json.Serialization;
using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;

namespace Nirvana.Public.Entities.NEL;

public class EntityServerDetail {
    public EntityServerDetail(string id)
    {
        Exception? exception = null;
        var threads = new List<Thread> {
            new(() => {
                try {
                    if (exception != null) {
                        return;
                    }

                    var item = NPFLauncher.GetNetGameDetailByIdAsync(id).Result;
                    CacheManager.ClearCacheImage(item);
                    Set(item);
                } catch (Exception e) {
                    exception = e;
                }
            }),
            new(() => {
                try {
                    if (exception != null) {
                        return;
                    }

                    Set(NPFLauncher.GetNetGameServerAddressAsync(id).Result);
                } catch (Exception e) {
                    exception = e;
                }
            })
        };
        foreach (var thread in threads) {
            thread.Start();
        }

        foreach (var thread in threads) {
            thread.Join();
        }

        if (exception != null) {
            throw exception;
        }
    }

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

    private void Set(EntityQueryNetGameDetailItem? data)
    {
        // 成功检测
        if (data == null) {
            throw new ErrorCodeException(ErrorCode.LogInNot);
        }

        Id = data.EntityId;
        Name = data.Name;
        Author = data.DeveloperName;
        // unix 时间戳 转换为 文本
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(data.PublishTime).ToString("yyyy-MM-dd");
        GameVersion = "";
        foreach (var version in data.McVersionList) {
            GameVersion += version.Name + ", ";
        }

        // 删除最后一个逗号
        GameVersion = GameVersion.TrimEnd(',', ' ');
        FullDescription = data.DetailDescription;
        BriefImageUrls = data.BriefImageUrls;
    }

    private void Set(EntityNetGameServerAddress data)
    {
        if (data == null) {
            throw new ErrorCodeException(ErrorCode.AddressError);
        }

        Address = data.Host;
        if (data.Port != 25565) Address += $":{data.Port}";
    }
}