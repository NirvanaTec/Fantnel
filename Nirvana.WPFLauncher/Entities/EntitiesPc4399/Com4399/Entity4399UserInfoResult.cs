using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesPc4399.Com4399;

public class Entity4399UserInfoResult {
    [JsonPropertyName("uid")]
    public long Uid { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}