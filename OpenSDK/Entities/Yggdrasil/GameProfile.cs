using System.Text.Json;

namespace OpenSDK.Entities.Yggdrasil;

public class GameProfile
{
    public required string GameId { get; init; }
    public required string GameVersion { get; init; }
    public required string BootstrapMd5 { get; init; }
    public required string DatFileMd5 { get; init; }
    public required ModList? Mods { get; init; }
    public required UserProfile User { get; init; }

    public string GetModInfo()
    {
        return Mods == null ? string.Empty : JsonSerializer.Serialize(Mods);
    }
    
}