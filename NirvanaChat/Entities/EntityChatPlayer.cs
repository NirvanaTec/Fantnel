using System.Text.Json.Serialization;

namespace NirvanaChat.Entities;

public class EntityChatPlayer {
    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty; // 账户

    [JsonPropertyName("friendlyMode")]
    public bool FriendlyMode { get; set; } // 友好模式

    [JsonPropertyName("players")]
    public EntityChatJoin[] Players { get; set; } = []; // 玩家列表

    public bool IsNullOrEmptyByAccount()
    {
        return string.IsNullOrEmpty(Account);
    }
}