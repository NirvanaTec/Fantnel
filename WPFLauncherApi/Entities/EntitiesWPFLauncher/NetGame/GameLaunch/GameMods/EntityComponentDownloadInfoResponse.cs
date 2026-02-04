using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntityComponentDownloadInfoResponse {
    [JsonPropertyName("entity_id")] public required string EntityId { get; set; }

    [JsonPropertyName("item_id")] public required string ItemId { get; set; }

    [JsonPropertyName("user_id")] public required string UserId { get; set; }

    [JsonPropertyName("itype")] public required int IType { get; set; }

    [JsonPropertyName("mtypeid")] public required int MTypeId { get; set; }

    [JsonPropertyName("stypeid")] public required int STypeId { get; set; }

    [JsonPropertyName("sub_entities")]
    public required List<EntityComponentDownloadInfoResponseSub> SubEntities { get; set; }

    [JsonPropertyName("sub_mod_list")] public required List<ulong> SubModList { get; set; }
}