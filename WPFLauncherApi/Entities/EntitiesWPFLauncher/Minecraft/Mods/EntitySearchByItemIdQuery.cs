using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft.Mods;

public class EntitySearchByItemIdQuery {
    [JsonPropertyName("item_id")]
    public required string ItemId { get; set; }

    [JsonPropertyName("length")]
    public required int Length { get; set; }

    [JsonPropertyName("offset")]
    public required int Offset { get; set; }
}