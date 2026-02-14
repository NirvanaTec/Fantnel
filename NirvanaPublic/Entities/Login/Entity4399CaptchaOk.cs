using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Login;

public class Entity4399CaptchaOk {
    // [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("captcha")]
    public string? Captcha { get; set; }
}