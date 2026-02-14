using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399.Com4399;

public class Entity4399VipInfo {
    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}