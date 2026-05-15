using System.Text.Json.Serialization;

namespace Nirvana.WPFLauncher.Entities.MPay;

public class EntityDeviceResponse {
    [JsonPropertyName("device")]
    public EntityDevice EntityDevice { get; set; } = new();
}