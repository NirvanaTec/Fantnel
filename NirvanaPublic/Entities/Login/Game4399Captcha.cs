using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Login;

public class Game4399Captcha
{
    [JsonPropertyName("captcha_url")] public string? CaptchaUrl { get; set; }
    [JsonPropertyName("session_id")] public string? SessionId { get; set; }

    public bool IsEmpty()
    {
        return CaptchaUrl == null || SessionId == null;
    }
}