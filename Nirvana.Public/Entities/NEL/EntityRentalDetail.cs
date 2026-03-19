using System.Text.Json.Serialization;
using Nirvana.Public.Manager;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.RentalGame;
using Nirvana.WPFLauncher.Protocol;
using NirvanaAPI.Utils.CodeTools;

namespace Nirvana.Public.Entities.NEL;

public class EntityRentalDetail {
    public EntityRentalDetail(string id)
    {
        Exception? exception = null;
        var threads = new List<Thread> {
            new(() => {
                try {
                    if (exception != null) {
                        return;
                    }

                    var item = NPFLauncher.GetRentalGameDetailsAsync(id).Result;
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

                    Set(NPFLauncher.GetGameRentalAddressAsync(id).Result);
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

    [JsonPropertyName("mc_version")]
    public string? McVersion { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("player_count")]
    public uint PlayerCount { get; set; }

    [JsonPropertyName("capacity")]
    public uint Capacity { get; set; }

    [JsonPropertyName("brief_summary")]
    public string? BriefSummary { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("server_type")]
    public string? ServerType { get; set; }

    private void Set(EntityRentalGameDetails entity)
    {
        if (entity == null) {
            throw new ErrorCodeException(ErrorCode.IdError);
        }

        Id = entity.EntityId;
        Name = entity.ServerName;
        ImageUrl = entity.ImageUrl;
        Capacity = entity.Capacity;
        McVersion = entity.McVersion;
        ServerType = entity.ServerType;
        PlayerCount = entity.PlayerCount;
        BriefSummary = entity.BriefSummary;
    }

    private void Set(EntityRentalGameServerAddress data)
    {
        if (data == null) {
            throw new ErrorCodeException(ErrorCode.AddressError);
        }

        Address = data.McServerHost;
        if (data.McServerPort != 25565) Address += $":{data.McServerPort}";
    }
}