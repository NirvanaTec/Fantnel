using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Plugin;

public class EntityPluginDependency
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}