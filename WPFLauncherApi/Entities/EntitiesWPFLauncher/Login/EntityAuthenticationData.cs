using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityAuthenticationData
{
    [JsonPropertyName("sa_data")] public required string SaData { get; set; }

    [JsonPropertyName("sauth_json")] public required string AuthJson { get; set; }

    [JsonPropertyName("version")] public required EntityAuthenticationVersion Version { get; set; }

    [JsonPropertyName("sdkuid")] public string? SdkUid { get; set; }

    [JsonPropertyName("aid")] public required string Aid { get; set; }

    [JsonPropertyName("hasMessage")] public bool HasMessage { get; set; }

    [JsonPropertyName("hasGmail")] public bool HasGmail { get; set; }

    [JsonPropertyName("otp_token")] public required string OtpToken { get; set; }

    [JsonPropertyName("otp_pwd")] public string? OtpPwd { get; set; }

    [JsonPropertyName("lock_time")] public int LockTime { get; set; }

    [JsonPropertyName("env")] public string? Env { get; set; }

    [JsonPropertyName("min_engine_version")]
    public string? MinEngineVersion { get; set; }

    [JsonPropertyName("min_patch_version")]
    public string? MinPatchVersion { get; set; }

    [JsonPropertyName("verify_status")] public int VerifyStatus { get; set; }

    [JsonPropertyName("unisdk_login_json")]
    public string? UniSdkLoginJson { get; set; }

    [JsonPropertyName("token")] public string? Token { get; set; }

    [JsonPropertyName("is_register")] public bool IsRegister { get; set; } = true;

    [JsonPropertyName("entity_id")] public string? EntityId { get; set; }
}