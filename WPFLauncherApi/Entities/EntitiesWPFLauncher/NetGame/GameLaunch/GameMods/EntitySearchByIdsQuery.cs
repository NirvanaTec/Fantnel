using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntitySearchByIdsQuery {
    [JsonPropertyName("item_id_list")]
    public required List<ulong> ItemIdList { get; set; }
}