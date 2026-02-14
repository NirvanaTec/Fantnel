using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;

public class EntityQueryRentalGameDetail {
    [JsonPropertyName("server_id")]
    public string ServerId { get; set; } = string.Empty;
}