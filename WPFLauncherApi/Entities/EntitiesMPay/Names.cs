using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesMPay;

// ReSharper disable once ClassNeverInstantiated.Global
public class Names
{
    [JsonPropertyName("en")] public string En { get; set; }
}