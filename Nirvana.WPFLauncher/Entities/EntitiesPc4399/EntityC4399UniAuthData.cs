using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesPc4399;

public class EntityC4399UniAuthData {
    [JsonPropertyName("sdk_login_data")]
    public string SdkLoginData { get; set; } = string.Empty;
}