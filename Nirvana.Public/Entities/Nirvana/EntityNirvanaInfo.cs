using System.Text.Json.Serialization;

namespace Nirvana.Public.Entities.Nirvana;

public class EntityNirvanaInfo{
    [JsonPropertyName("code")]
    public int? Code { get; set; }
    
    [JsonPropertyName("days")]
    public required double Days { get; set; }

}