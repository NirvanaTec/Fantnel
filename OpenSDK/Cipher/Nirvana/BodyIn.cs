using System.Text.Json.Serialization;

namespace OpenSDK.Cipher.Nirvana;

public class BodyIn
{
    [JsonPropertyName("body")] public required string Body { get; set; }
}