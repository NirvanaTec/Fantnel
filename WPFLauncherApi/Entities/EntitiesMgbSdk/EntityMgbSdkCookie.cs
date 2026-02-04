using System.Text.Json;
using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesMPay;

namespace WPFLauncherApi.Entities.EntitiesMgbSdk;

public class EntityMgbSdkCookie {
    [JsonPropertyName("timestamp")] public required string Timestamp { get; set; }

    [JsonPropertyName("userid")] public required string UserId { get; set; }

    [JsonPropertyName("realname")] public string RealName { get; set; } = "{\"realname_type\":\"0\"}";

    [JsonPropertyName("gameid")] public required string GameId { get; set; }

    [JsonPropertyName("login_channel")] public required string LoginChannel { get; set; }

    [JsonPropertyName("app_channel")] public required string AppChannel { get; set; }

    [JsonPropertyName("platform")] public string Platform { get; set; } = "pc";

    [JsonPropertyName("sdkuid")] public required string SdkUid { get; set; }

    [JsonPropertyName("sessionid")] public required string SessionId { get; set; }

    [JsonPropertyName("sdk_version")] public string SdkVersion { get; set; } = "1.0.0";

    [JsonPropertyName("udid")] public required string Udid { get; set; }

    [JsonPropertyName("deviceid")] public required string DeviceId { get; set; }

    [JsonPropertyName("aim_info")] public string AimInfo { get; set; } = JsonSerializer.Serialize(new EntityAimInfo());

    [JsonPropertyName("client_login_sn")] public required string ClientLoginSn { get; set; }

    [JsonPropertyName("gas_token")] public string GasToken { get; set; } = string.Empty;

    [JsonPropertyName("source_platform")] public string SourcePlatform { get; set; } = "pc";

    [JsonPropertyName("ip")] public string Ip { get; set; } = "127.0.0.1";
}