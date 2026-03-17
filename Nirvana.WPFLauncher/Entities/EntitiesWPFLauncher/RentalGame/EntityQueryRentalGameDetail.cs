using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.RentalGame;

public class EntityQueryRentalGameDetail {
    [JsonPropertyName("server_id")]
    public string ServerId { get; set; } = string.Empty;
}