using System.Text.Json.Serialization;

namespace Codexus.Development.SDK.Entities;

public class BodyIn {
    [JsonPropertyName("body")]
    public required string Body { get; set; }
}