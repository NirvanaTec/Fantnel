using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399;

public class EntityMgbSdkSAuthJson
{
    [JsonPropertyName("aim_info")]
    // ReSharper disable once UnusedMember.Global
    public string AimInfo { get; set; } = "{\"aim\":\"127.0.0.1\",\"tz\":\"+0800\",\"tzid\":\"\",\"country\":\"CN\"}";

    [JsonPropertyName("app_channel")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string AppChannel { get; set; }

    [JsonPropertyName("client_login_sn")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string ClientLoginSn { get; set; }

    [JsonPropertyName("deviceid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string DeviceId { get; set; }

    [JsonPropertyName("gameid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string GameId { get; set; }

    [JsonPropertyName("gas_token")]
    // ReSharper disable once UnusedMember.Global
    public string GasToken { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    // ReSharper disable once UnusedMember.Global
    public string Ip { get; set; } = "127.0.0.1";

    [JsonPropertyName("login_channel")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string LoginChannel { get; set; }

    [JsonPropertyName("platform")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Platform { get; set; } = "pc";

    [JsonPropertyName("realname")]
    // ReSharper disable once UnusedMember.Global
    public string RealName { get; set; } = "{\"realname_type\":\"0\"}";

    [JsonPropertyName("sdk_version")]
    // ReSharper disable once UnusedMember.Global
    public string SdkVersion { get; set; } = "1.0.0";

    [JsonPropertyName("sdkuid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string SdkUid { get; set; }

    [JsonPropertyName("sessionid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string SessionId { get; set; }

    [JsonPropertyName("source_platform")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string SourcePlatform { get; set; } = "pc";

    [JsonPropertyName("timestamp")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Timestamp { get; set; }

    [JsonPropertyName("udid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Udid { get; set; }

    [JsonPropertyName("userid")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string UserId { get; set; }
}