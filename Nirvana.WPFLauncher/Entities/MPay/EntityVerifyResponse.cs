using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntityVerifyResponse {
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("verify_url")]
    public string VerifyUrl { get; set; } = string.Empty;
}