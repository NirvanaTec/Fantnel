using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399;

public class EntityC4399UniAuthData
{
    [JsonPropertyName("sdk_login_data")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string SdkLoginData { get; set; } = string.Empty;
}