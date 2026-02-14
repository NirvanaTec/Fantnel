using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityNetGameServerAddress {
    [JsonPropertyName("ip")]
    public required string Host { get; init; }


    [JsonPropertyName("port")]
    public required int Port { get; init; }
}