using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399;

public class EntityMgbSdkSAuthJson {
    [JsonPropertyName("aim_info")]
    public string AimInfo { get; set; } = "{\"aim\":\"127.0.0.1\",\"tz\":\"+0800\",\"tzid\":\"\",\"country\":\"CN\"}";

    [JsonPropertyName("app_channel")] public required string AppChannel { get; set; }

    [JsonPropertyName("client_login_sn")] public required string ClientLoginSn { get; set; }

    [JsonPropertyName("deviceid")] public required string DeviceId { get; set; }

    [JsonPropertyName("gameid")] public required string GameId { get; set; }

    [JsonPropertyName("gas_token")] public string GasToken { get; set; } = string.Empty;

    [JsonPropertyName("ip")] public string Ip { get; set; } = "127.0.0.1";

    [JsonPropertyName("login_channel")] public required string LoginChannel { get; set; }

    [JsonPropertyName("platform")] public string Platform { get; set; } = "pc";

    [JsonPropertyName("realname")] public string RealName { get; set; } = "{\"realname_type\":\"0\"}";

    [JsonPropertyName("sdk_version")] public string SdkVersion { get; set; } = "1.0.0";

    [JsonPropertyName("sdkuid")] public required string SdkUid { get; set; }

    [JsonPropertyName("sessionid")] public required string SessionId { get; set; }

    [JsonPropertyName("source_platform")] public string SourcePlatform { get; set; } = "pc";

    [JsonPropertyName("timestamp")] public required string Timestamp { get; set; }

    [JsonPropertyName("udid")] public required string Udid { get; set; }

    [JsonPropertyName("userid")] public required string UserId { get; set; }
}