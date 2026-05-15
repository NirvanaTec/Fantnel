using System;
using System.Text.Json.Serialization;

namespace Nirvana.DevPlugin.Entities;

public class InterceptorConfig {
    [JsonIgnore]
    public Action<InterceptorConfig, string>? OnJoinServer;

    [JsonPropertyName("is_rental")]
    public required bool IsRental { get; init; }

    [JsonPropertyName("local_address")]
    public string LocalAddress { get; init; } = "localhost";

    [JsonPropertyName("local_port")]
    public required int LocalPort { get; init; }

    [JsonPropertyName("nickname")]
    public required string NickName { get; init; }

    [JsonPropertyName("forward_address")]
    public required string ForwardAddress { get; set; }

    [JsonPropertyName("forward_port")]
    public required int ForwardPort { get; set; }

    [JsonPropertyName("server_name")]
    public required string ServerName { get; init; }

    [JsonPropertyName("server_version")]
    public required string ServerVersion { get; init; }

    [JsonPropertyName("mod_info")]
    public required string ModInfo { get; init; }

    [JsonPropertyName("game_id")]
    public required string GameId { get; init; }
}