using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher;

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class EntityWPFLauncher<T> : EntityWPFResponse
{
    [JsonPropertyName("entity")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public T? Data { get; set; }

    public T SafeEntity()
    {
        return Data ?? throw new EntityX19Exception(Message, this);
    }
}