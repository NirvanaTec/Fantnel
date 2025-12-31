using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Login;

// 登录信息
public class EntityUserInfo
{
    [JsonPropertyName("token")] public string? Token { get; set; }
    [JsonPropertyName("userId")] public string? UserId { get; set; }
}