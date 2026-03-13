
using System.Text.Json.Serialization;

namespace Codexus.Development.SDK.Entities;

public class InterceptorConfig
{
    [JsonPropertyName("is_rental")]
    public required bool IsRental { get; set; }

    [JsonPropertyName("local_address")]
    public required string LocalAddress { get; set; }

    [JsonPropertyName("local_port")]
    public required int LocalPort { get; set; }

    [JsonPropertyName("nickname")]
    public required string NickName { get; set; }

    [JsonPropertyName("forward_address")]
    public required string ForwardAddress { get; set; }

    [JsonPropertyName("forward_port")]
    public required int ForwardPort { get; set; }

    [JsonPropertyName("server_name")]
    public required string ServerName { get; set; }

    [JsonPropertyName("server_version")]
    public required string ServerVersion { get; set; }

    [JsonPropertyName("mod_info")]
    public required string ModInfo { get; set; }

    [JsonPropertyName("game_id")]
    public required string GameId { get; set; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    [JsonPropertyName("user_token")]
    public required string UserToken { get; set; }

    [JsonIgnore]
    public Action<InterceptorConfig, string>? OnJoinServer { get; set; }

    [JsonIgnore]
    public EntitySocks5 Socks5 { get; set; } = new EntitySocks5();

    public InterceptorConfig Clone()
    {
        return new InterceptorConfig
        {
            IsRental = IsRental,
            LocalAddress = LocalAddress,
            LocalPort = LocalPort,
            NickName = NickName,
            ForwardAddress = ForwardAddress,
            ForwardPort = ForwardPort,
            ServerName = ServerName,
            ServerVersion = ServerVersion,
            ModInfo = ModInfo,
            GameId = GameId,
            UserId = UserId,
            UserToken = UserToken,
            OnJoinServer = OnJoinServer,
            Socks5 = Socks5
        };
    }
}