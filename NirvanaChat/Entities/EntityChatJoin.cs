using System.Text.Json.Serialization;
using Codexus.Development.SDK.Connection;

namespace NirvanaChat.Entities;

public class EntityChatJoin {
    [JsonPropertyName("mode")] public string Mode { get; set; } = "join";

    [JsonPropertyName("nickName")] public required string NickName { get; set; }

    [JsonPropertyName("gameId")] public required string GameId { get; set; }

    [JsonPropertyName("entityId")] public int EntityId { get; set; } = -1;


    public bool Equals(string gameId)
    {
        return GameId.Equals(gameId);
    }

    public bool Equals(string gameId, string nickName)
    {
        return Equals(gameId) && NickName.Equals(nickName);
    }

    public bool Equals(GameConnection gameConnection)
    {
        return Equals(gameConnection.GameId, gameConnection.NickName);
    }

    public bool Equals(EntityChatJoin entityChatJoin)
    {
        return Equals(entityChatJoin.GameId, entityChatJoin.NickName);
    }
}