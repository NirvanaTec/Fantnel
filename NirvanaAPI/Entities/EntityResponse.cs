using System.Text.Json.Serialization;
using NirvanaAPI.Utils;

namespace NirvanaAPI.Entities;

public class EntityResponseBase {
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("msg")]
    [JsonConverter(typeof(FirstStringConverter))]
    public string? Msg { get; set; }
}

public class EntityResponse<T> : EntityResponseBase {
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class EntityInfo {
    [JsonPropertyName("versions")]
    public string[]? Versions { get; init; }

    [JsonPropertyName("disabled_version")]
    public string[] DisabledVersions { get; init; } = [];

    [JsonPropertyName("ad1")]
    public Advertisement? Ad1 { get; init; }

    [JsonPropertyName("ad2")]
    public Advertisement? Ad2 { get; init; }

    [JsonPropertyName("ad3")]
    public Advertisement? Ad3 { get; init; }

    [JsonPropertyName("crcSalt")]
    public string? CrcSalt { get; init; }

    [JsonPropertyName("gameVersion")]
    public string? GameVersion { get; init; }
    
    [JsonPropertyName("shopUrl")]
    public string? ShopUrl { get; init; }
}

public class Advertisement {
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}