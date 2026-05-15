using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntityMPayUserResponse {
    [JsonPropertyName("force_pwd")]
    public bool ForcePwd { get; set; }

    [JsonPropertyName("verify_status")]
    public EntityVerifyStatus? VerifyStatus { get; set; }

    [JsonPropertyName("user")]
    public required EntityMPayUser User { get; set; } = new();
}