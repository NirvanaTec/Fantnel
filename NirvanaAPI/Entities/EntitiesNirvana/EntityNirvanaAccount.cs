using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities.EntitiesNirvana;

public class EntityNirvanaAccount {
    [JsonPropertyName("account")] public string Account { get; set; } = string.Empty;

    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"&account={Account}&online={Token}";
    }

    public bool IsNullOrEmpty()
    {
        return string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Account);
    }

    public void Logout()
    {
        Account = string.Empty;
        Token = string.Empty;
    }
}