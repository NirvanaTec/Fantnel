using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

public class EntityX19CookieRequest {
    [JsonPropertyName("sauth_json")]
    public required string Json { get; init; }
}