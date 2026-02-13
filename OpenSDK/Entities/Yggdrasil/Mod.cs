using System.Text.Json.Serialization;

namespace OpenSDK.Entities.Yggdrasil;

// ReSharper disable once ClassNeverInstantiated.Global
public class Mod {
    [JsonPropertyName("modPath")] public required string ModPath { get; set; }


    [JsonPropertyName("name")] public required string Name { get; set; } = string.Empty;


    [JsonPropertyName("id")] public required string Id { get; set; }


    [JsonPropertyName("iid")] public required string Iid { get; set; }


    [JsonPropertyName("md5")] public required string Md5 { get; set; }


    [JsonPropertyName("version")] public required string Version { get; set; } = string.Empty;
}