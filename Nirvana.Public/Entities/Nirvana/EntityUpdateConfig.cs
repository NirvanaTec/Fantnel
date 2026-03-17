using System.Text.Json.Nodes;

namespace Nirvana.Public.Entities.Nirvana;

public class EntityUpdateConfig {
    
    public required string Mode;
    public string Name = "Resource";
    public bool FailureLog = true;
    public Action<JsonArray>? OnSuccess = null;


}