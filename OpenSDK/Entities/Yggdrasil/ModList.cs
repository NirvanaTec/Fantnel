using System.Text.Json.Serialization;

namespace OpenSDK.Entities.Yggdrasil;

public class ModList
{
    [JsonPropertyName("mods")] public List<Mod> Mods { get; init; } = [];
}