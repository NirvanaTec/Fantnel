using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;

// ReSharper disable once ClassNeverInstantiated.Global
public class EntityNetGameServerAddress {
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("ip")] public required string Host { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("port")] public required int Port { get; init; }
}