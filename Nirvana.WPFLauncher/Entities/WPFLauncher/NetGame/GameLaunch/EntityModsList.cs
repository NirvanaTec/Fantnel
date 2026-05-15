using System.Collections.Generic;
using System.Text.Json.Serialization;
using Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameLaunch.GameMods;

namespace Nirvana.WPFLauncher.Entities.WPFLauncher.NetGame.GameLaunch;

public class EntityModsList {
    [JsonPropertyName("mods")]
    public List<EntityModsInfo> Mods { get; set; } = [];
}