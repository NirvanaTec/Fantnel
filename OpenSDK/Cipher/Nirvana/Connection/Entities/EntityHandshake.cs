using System.Text.Json.Serialization;

namespace OpenSDK.Cipher.Nirvana.Connection.Entities;

public class EntityHandshake
{
    [JsonPropertyName("handshakeBody")] public required string HandshakeBody { get; set; }
}