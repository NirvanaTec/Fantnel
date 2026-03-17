using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.Login;

public class EntityX19CookieRequest {
    [JsonPropertyName("sauth_json")]
    public required string Json { get; init; }
}