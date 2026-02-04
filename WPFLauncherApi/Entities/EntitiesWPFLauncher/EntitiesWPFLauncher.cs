using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher;

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class EntitiesWPFLauncher<T> : EntityWPFResponse {
    // [JsonPropertyName("details")] public string Details { get; set; } = string.Empty;

    [JsonPropertyName("entities")] public T[] Data { get; init; } = [];

    // [JsonPropertyName("total")]
    // [JsonConverter(typeof(NetEaseStringConverter))]
    // public int Total { get; set; }
}