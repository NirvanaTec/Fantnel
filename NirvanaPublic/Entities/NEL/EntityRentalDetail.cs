using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameDetails;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;
using WPFLauncherApi.Utils.CodeTools;

namespace NirvanaPublic.Entities.NEL;

public class EntityRentalDetail
{
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
    
    public void Set(EntityRentalGameDetails entity)
    {
        if (entity == null) throw new ErrorCodeException(ErrorCode.IdError);
        Id = entity.EntityId;
        Name = entity.ServerName;
        ImageUrl = entity.ImageUrl;
        Capacity = entity.Capacity;
        McVersion = entity.McVersion;
        ServerType = entity.ServerType;
        PlayerCount = entity.PlayerCount;
        BriefSummary = entity.BriefSummary;
    }

    public void Set(EntityRentalGameServerAddress data)
    {
        if (data == null) throw new ErrorCodeException(ErrorCode.AddressError);
        Address = data.McServerHost;
        if (data.McServerPort != 25565) Address += $":{data.McServerPort}";
    }
}