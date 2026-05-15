using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameLaunch.GameMods;

public class EntitySearchByIdsQuery {
    [JsonPropertyName("item_id_list")]
    public required List<ulong> ItemIdList { get; set; }
}