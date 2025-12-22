using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Plugin;

public class EntityPlugin
{
    [JsonPropertyName("detailDescription")]
    public string? DetailDescription { get; set; }

    [JsonPropertyName("downloadCount")] public int? DownloadCount { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("logoUrl")] public string? LogoUrl { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("publishDate")] public string? PublishDate { get; set; }

    [JsonPropertyName("publisher")] public string? Publisher { get; set; }

    [JsonPropertyName("shortDescription")] public string? ShortDescription { get; set; }

    [JsonPropertyName("version")] public string? Version { get; set; }

    [JsonPropertyName("dependencies")] public EntityPluginDependency[]? Dependencies { get; set; }
}