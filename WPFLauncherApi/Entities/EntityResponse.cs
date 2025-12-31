using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities;

public class EntityResponse<T>
{
    [JsonPropertyName("code")] public int? Code { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }
    [JsonPropertyName("msg")] public string? Msg { get; set; }
}

public class EntityInfo
{
    [JsonPropertyName("versions")] public string[]? Versions { get; init; }

    [JsonPropertyName("ad1")] public Advertisement? Ad1 { get; init; }

    [JsonPropertyName("ad2")] public Advertisement? Ad2 { get; init; }

    [JsonPropertyName("ad3")] public Advertisement? Ad3 { get; init; }

    [JsonPropertyName("crcSalt")] public string? CrcSalt { get; init; }

    [JsonPropertyName("gameVersion")] public string? GameVersion { get; init; }
}

public class Advertisement
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("text")] public string? Text { get; set; }
}