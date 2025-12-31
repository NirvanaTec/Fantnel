using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesPc4399;

public class EntityC4399UniAuth
{
    [JsonPropertyName("data")] public EntityC4399UniAuthData Data { get; init; } = new();
}