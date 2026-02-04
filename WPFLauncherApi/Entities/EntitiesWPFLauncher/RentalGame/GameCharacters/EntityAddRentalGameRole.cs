using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame.GameCharacters;

public class EntityAddRentalGameRole {
    [JsonPropertyName("server_id")] public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("create_ts")] public int CreateTs { get; set; }

    [JsonPropertyName("is_online")] public bool IsOnline { get; set; }

    [JsonPropertyName("status")] public int Status { get; set; }
}