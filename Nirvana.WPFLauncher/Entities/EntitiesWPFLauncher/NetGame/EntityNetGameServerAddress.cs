using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame;

public class EntityNetGameServerAddress {
    [JsonPropertyName("ip")]
    public required string Host { get; init; }


    [JsonPropertyName("port")]
    public required int Port { get; init; }
}