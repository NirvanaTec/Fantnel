using System.Text.Json.Serialization;

namespace OpenSDK.Entities.Yggdrasil;

public class YggdrasilServer
{
    [JsonPropertyName("IP")] public required string Ip { get; set; }
    [JsonPropertyName("Port")] public required int Port { get; set; }
    [JsonPropertyName("ServerType")] public required string ServerType { get; set; }
}