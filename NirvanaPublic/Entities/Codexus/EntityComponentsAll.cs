using System.Text.Json.Serialization;

namespace NirvanaPublic.Entities.Codexus;

public class EntityComponentsAll
{
    // [JsonPropertyName("total")]
    // public long? Total { get; set; }
    [JsonPropertyName("items")]
    public EntityComponents[]? Items { get; set; }
}