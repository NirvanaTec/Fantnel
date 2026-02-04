using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityAuthenticationOtp {
    [JsonPropertyName("entity_id")] public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("account")] public string Account { get; set; } = string.Empty;

    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;

    [JsonPropertyName("sead")] public string Sead { get; set; } = string.Empty;

    [JsonPropertyName("hasMessage")] public bool HasMessage { get; set; }

    [JsonPropertyName("aid")] public string Aid { get; set; } = string.Empty;

    [JsonPropertyName("sdkuid")] public string SdkUid { get; set; } = string.Empty;

    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("unisdk_login_json")]
    public string UniSdkLoginJson { get; set; } = string.Empty;

    [JsonPropertyName("verify_status")] public int VerifyStatus { get; set; }

    [JsonPropertyName("hasGmail")] public bool HasGmail { get; set; }

    [JsonPropertyName("is_register")] public bool IsRegister { get; set; }

    [JsonPropertyName("env")] public string Env { get; set; } = string.Empty;

    [JsonPropertyName("last_server_up_time")]
    public long LastServerUpTime { get; set; }

    [JsonPropertyName("min_engine_version")]
    public string MinEngineVersion { get; set; } = string.Empty;

    [JsonPropertyName("min_patch_version")]
    public string MinPatchVersion { get; set; } = string.Empty;
}