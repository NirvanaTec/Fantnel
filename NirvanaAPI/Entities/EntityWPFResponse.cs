using System.Text.Json.Serialization;
using NirvanaAPI.Manager;

namespace NirvanaAPI.Entities;

// ReSharper disable once InconsistentNaming
public class EntityWPFResponse {
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    protected void SafeEntity()
    {
        // Token 过期 | 账号被顶
        if (Code is 10 or 22) {
            InfoManager.DeleteAccount(this);
        }
    }
}