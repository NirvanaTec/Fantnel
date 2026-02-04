using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.RentalGame;

public class EntityRentalGameDetails {
    [JsonPropertyName("entity_id")] public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("brief_summary")] public string BriefSummary { get; set; } = string.Empty;

    [JsonPropertyName("mc_version")] public string McVersion { get; set; } = string.Empty;

    [JsonPropertyName("capacity")] public uint Capacity { get; set; }

    [JsonPropertyName("player_count")] public uint PlayerCount { get; set; }

    [JsonPropertyName("image_url")] public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("server_type")] public string? ServerType { get; set; }

    [JsonPropertyName("server_name")] public string ServerName { get; set; } = string.Empty;
}