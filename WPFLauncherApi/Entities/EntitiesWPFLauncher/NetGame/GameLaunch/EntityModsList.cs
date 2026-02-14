using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch;

public class EntityModsList {
    [JsonPropertyName("mods")]
    public List<EntityModsInfo> Mods { get; set; } = [];
}