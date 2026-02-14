using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame;

public class EntityNetGameRequest {
    [JsonPropertyName("available_mc_versions")]
    public required string[] AvailableMcVersions { get; set; }

    [JsonPropertyName("item_type")]
    public required int ItemType { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("offset")]
    public required int Offset { get; set; }

    [JsonPropertyName("master_type_id")]
    public required string MasterTypeId { get; set; }

    [JsonPropertyName("secondary_type_id")]
    public required string SecondaryTypeId { get; set; }
}