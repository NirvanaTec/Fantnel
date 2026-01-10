using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher.NetGame.GameLaunch.GameMods;

public class EntityModsInfo
{
    [JsonPropertyName("modPath")] public required string ModPath { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; } = "";

    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("iid")] public required string Iid { get; set; }

    [JsonPropertyName("md5")] public required string Md5 { get; set; }

    [JsonPropertyName("version")] public required string Version { get; set; } = "";
}