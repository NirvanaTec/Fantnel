using System.Text.Json.Serialization;
using Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

namespace Nirvana.WPFLauncher.Entities.EntitiesWPFLauncher.NetGame.GameLaunch;

public class EntityModsList {
    [JsonPropertyName("mods")]
    public List<EntityModsInfo> Mods { get; set; } = [];
}