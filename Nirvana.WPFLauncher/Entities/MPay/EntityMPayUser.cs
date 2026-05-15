using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntityMPayUser {
    [JsonPropertyName("ext_access_token")]
    public string ExtAccessToken { get; set; } = string.Empty;

    [JsonPropertyName("realname_verify_status")]
    public int RealNameVerifyStatus { get; set; }

    [JsonPropertyName("login_channel")]
    public string LoginChannel { get; set; } = string.Empty;

    [JsonPropertyName("realname_status")]
    public int RealNameStatus { get; set; }

    [JsonPropertyName("related_login_status")]
    public int RelatedLoginStatus { get; set; }

    [JsonPropertyName("need_mask")]
    public bool NeedMask { get; set; }

    [JsonPropertyName("mobile_bind_status")]
    public int MobileBindStatus { get; set; }

    [JsonPropertyName("mask_related_mobile")]
    public string MaskRelatedMobile { get; set; } = string.Empty;

    [JsonPropertyName("display_username")]
    public string DisplayUsername { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("client_username")]
    public string ClientUsername { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("need_aas")]
    public bool NeedAas { get; set; }

    [JsonPropertyName("login_type")]
    public int LoginType { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("pc_ext_info")]
    public EntityPcExtInfo EntityPcExtInfo { get; set; } = new();
}