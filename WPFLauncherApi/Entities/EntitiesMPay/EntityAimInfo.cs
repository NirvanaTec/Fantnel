using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesMPay;

public class EntityAimInfo {
    [JsonPropertyName("aim")] public string Aim { get; set; } = "127.0.0.1";
    [JsonPropertyName("country")] public string Country { get; set; } = "CN";
    [JsonPropertyName("tz")] public string Tz { get; set; } = "+0800";
    [JsonPropertyName("tzid")] public string TzId { get; set; } = string.Empty;
}