using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399.Com4399;

public class Entity4399OAuthResult {
    [JsonPropertyName("login_url")]
    public string LoginUrl { get; set; } = string.Empty;

    [JsonPropertyName("login_url_backup")]
    public string LoginUrlBackup { get; set; } = string.Empty;

    [JsonPropertyName("login_url_phone")]
    public string LoginUrlPhone { get; set; } = string.Empty;

    [JsonPropertyName("login_url_backup_phone")]
    public string LoginUrlBackupPhone { get; set; } = string.Empty;
}