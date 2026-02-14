using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399.Com4399;

public class Entity4399UserInfoResult {
    [JsonPropertyName("uid")]
    public long Uid { get; set; }

    [JsonPropertyName("idcard")]
    public string IdCard { get; set; } = string.Empty;

    [JsonPropertyName("reg_time")]
    public long RegTime { get; set; }

    [JsonPropertyName("validateState")]
    public int ValidateState { get; set; }

    [JsonPropertyName("bindedphone")]
    public string BindedPhone { get; set; } = string.Empty;

    [JsonPropertyName("idcard_state")]
    public int IdCardState { get; set; }

    [JsonPropertyName("realname")]
    public string RealName { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("full_bind_phone")]
    public string FullBindPhone { get; set; } = string.Empty;

    [JsonPropertyName("nck")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("avatar_middle")]
    public string AvatarMiddle { get; set; } = string.Empty;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string AuthCode { get; set; } = string.Empty;

    [JsonPropertyName("account_type")]
    public string AccountType { get; set; } = string.Empty;

    [JsonPropertyName("hello")]
    public string WelcomeMessage { get; set; } = string.Empty;

    [JsonPropertyName("idcard_editable")]
    public bool IdCardEditable { get; set; }

    [JsonPropertyName("id_checked")]
    public bool IdChecked { get; set; }

    [JsonPropertyName("id_checked_real")]
    public bool IdCheckedReal { get; set; }

    [JsonPropertyName("phone_bound")]
    public int PhoneBound { get; set; }

    [JsonPropertyName("activated")]
    public bool Activated { get; set; }

    [JsonPropertyName("vip_info")]
    public Entity4399VipInfo VipInfo { get; set; } = new();
}