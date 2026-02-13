using System.Text.Json.Serialization;

namespace Codexus.Development.SDK.Entities;

public class EntitySocks5 {
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }

    [JsonPropertyName("address")] public string Address { get; set; } = "127.0.0.1";

    [JsonPropertyName("port")] public int Port { get; set; } = 1080;

    [JsonPropertyName("username")] public string? Username { get; set; }

    [JsonPropertyName("password")] public string? Password { get; set; }
}