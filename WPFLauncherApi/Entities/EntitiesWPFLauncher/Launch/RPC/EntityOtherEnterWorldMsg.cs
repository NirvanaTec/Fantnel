using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.Launch.RPC;

public class EntityOtherEnterWorldMsg {
    [JsonPropertyName("id")]
    public short Id { get; set; }

    [JsonPropertyName("len")]
    public ushort Length { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("len1")]
    public ushort UuidLength { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;
}