using System.Text.Json.Serialization;
using NirvanaAPI.Entities.EntitiesNirvana;

namespace Nirvana.Chat.Entities.Packet;

public class EntityChatMessage : EntityText {
    [JsonPropertyName("extra")]
    public required List<EntityChatPart> Extra { get; set; }
}