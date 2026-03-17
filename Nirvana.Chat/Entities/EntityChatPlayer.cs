using System.Text.Json.Serialization;

namespace Nirvana.Chat.Entities;

public class EntityChatPlayer {
    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty; // 账户

    [JsonPropertyName("players")]
    public EntityChatJoin[] Players { get; set; } = []; // 玩家列表
}