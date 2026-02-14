using System.Text.Json.Serialization;

namespace NirvanaChat.Entities;

public class EntityChatConfig {
    [JsonPropertyName("heartbeats")]
    public List<string> Heartbeats { get; set; } = [];

    [JsonPropertyName("players")]
    public List<EntityChatPlayer> Players { get; set; } = [];

    // 随机获取一个心跳内容
    public string GetHeartbeat()
    {
        return Heartbeats.Count == 0 ? string.Empty : Heartbeats[new Random().Next(Heartbeats.Count)];
    }
}