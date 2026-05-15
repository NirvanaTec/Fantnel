using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntityVerifyStatus {
    [JsonPropertyName("need_sms")]
    public int NeedSms { get; set; }

    [JsonPropertyName("need_email")]
    public int NeedEmail { get; set; }

    [JsonPropertyName("need_passwd")]
    public int NeedPasswd { get; set; }

    [JsonPropertyName("need_real_name")]
    public int NeedRealName { get; set; }
}