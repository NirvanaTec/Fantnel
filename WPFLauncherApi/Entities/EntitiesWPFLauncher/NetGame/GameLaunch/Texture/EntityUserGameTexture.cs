using System.Text.Json.Serialization;
using WPFLauncherApi.Entities.EntitiesWPFLauncher.Minecraft;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.Texture;

public class EntityUserGameTexture
{
    [JsonPropertyName("entity_id")] public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("game_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnumGType GameType { get; set; }

    [JsonPropertyName("skin_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnumTextureType SkinType { get; set; }

    [JsonPropertyName("skin_id")] public string SkinId { get; set; } = string.Empty;

    [JsonPropertyName("skin_mode")] public int SkinMode { get; set; }

    [JsonPropertyName("client_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnumGameClientType ClientType { get; set; }
}