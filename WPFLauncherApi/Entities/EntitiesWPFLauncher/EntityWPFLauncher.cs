using System.Text.Json.Serialization;
using NirvanaAPI.Entities;
using NirvanaAPI.Manager;

namespace WPFLauncherApi.Entities.EntitiesWPFLauncher;

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class EntityWPFLauncher<T> : EntityWPFResponse {
    
    [JsonPropertyName("entity")]
    public T? Data { get; init; }

    public new T SafeEntity()
    {
        base.SafeEntity();
        return Data ?? throw new EntityX19Exception(Message, this);
    }
    
}