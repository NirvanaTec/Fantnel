using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.Login;

public class EntityAccount : EntityUserInfo {
    // 基础信息
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    // 识别信息
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    /**
     * 根据 基础信息 判断 是否 是 同一个账号
     * @param other 另一个账号
     * @return 是否 是 同一个账号
     */
    public bool Equals(EntityAccount other)
    {
        // cookie 用 值 判断
        if (Type == "cookie" && other.Type == "cookie") return Password == other.Password;
        // 账号 密码 登录类型 一致 则 认为 是 同一个账号
        return Account == other.Account && Type == other.Type && Password == other.Password;
    }

    public new string ToString()
    {
        return Type == "cookie"
            ? $"Type: {Type}, Password: {Password}"
            : $"Account: {Account}, Type: {Type}, Password: {Password}";
    }

    public void Update(EntityAccount account)
    {
        Token = account.Token;
        UserId = account.UserId;
    }

    public bool IsConfig()
    {
        return NirvanaConfig.Config.AutoLoginGame && (
            ("4399".Equals(Type) && NirvanaConfig.Config.AutoLoginGame4399) || 
            ("4399com".Equals(Type) && NirvanaConfig.Config.AutoLoginGame4399Com) || 
            ("163Email".Equals(Type) && NirvanaConfig.Config.AutoLoginGame163Email) || 
            ("cookie".Equals(Type) && NirvanaConfig.Config.AutoLoginGameCookie));
    }

}