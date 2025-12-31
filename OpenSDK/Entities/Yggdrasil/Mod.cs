using System.Text.Json.Serialization;

namespace OpenSDK.Entities.Yggdrasil;

// ReSharper disable once ClassNeverInstantiated.Global
public class Mod
{
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("modPath")] public required string ModPath { get; set; }
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("name")] public required string Name { get; set; } = string.Empty;
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("id")] public required string Id { get; set; }
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("iid")] public required string Iid { get; set; }
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("md5")] public required string Md5 { get; set; }
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("version")] public required string Version { get; set; } = string.Empty;
}