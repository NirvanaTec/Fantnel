using System.Text.Json.Serialization;
using Codexus.Cipher.Entities;

namespace NirvanaPublic.Entities.NEL;

public class Entity1<T> : EntityResponse
{
    [JsonPropertyName("entities")] public T? Data { get; set; }
}