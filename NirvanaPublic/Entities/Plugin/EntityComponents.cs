using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Plugin;

public class EntityComponents {
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("shortDescription")] public string? ShortDescription { get; set; }

    [JsonPropertyName("publisher")] public string? Publisher { get; set; }

    [JsonPropertyName("downloadCount")] public long? DownloadCount { get; set; }
}