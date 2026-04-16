using System.Text.Json.Serialization;

namespace Nirvana.Public.Entities.Nirvana;

public class EntityGeeTest {

    [JsonPropertyName("lot_number")]
    public required string LotNumber { get; set; }

    [JsonPropertyName("pass_token")]
    public required string PassToken { get; set; }

    [JsonPropertyName("gen_time")]
    public required string GenTime { get; set; }

    [JsonPropertyName("captcha_output")]
    public required string CaptchaOutput { get; set; }

    public string Get()
    {
        return "lot_number=" + LotNumber + "&pass_token=" + PassToken + "&gen_time=" + GenTime + "&captcha_output=" + CaptchaOutput;
    }
}