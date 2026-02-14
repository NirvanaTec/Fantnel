using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameSkin;

public class EntityFreeSkinListRequest {
    [JsonPropertyName("is_has")]
    public required bool IsHas { get; set; }

    [JsonPropertyName("item_type")]
    public required int ItemType { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("master_type_id")]
    public required int MasterTypeId { get; set; }

    [JsonPropertyName("offset")]
    public required int Offset { get; set; }

    [JsonPropertyName("price_type")]
    public required int PriceType { get; set; }

    [JsonPropertyName("secondary_type_id")]
    public required int SecondaryTypeId { get; set; }
}