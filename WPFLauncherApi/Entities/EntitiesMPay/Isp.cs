using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesMPay;

// ReSharper disable once ClassNeverInstantiated.Global
public class Isp
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("names")] public Names Names { get; set; }
}