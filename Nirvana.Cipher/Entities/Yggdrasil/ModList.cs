using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nirvana.Cipher.Entities.Yggdrasil;

public class ModList {
    [JsonPropertyName("mods")]
    public List<Mod> Mods { get; init; } = [];
}