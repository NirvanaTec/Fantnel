using System.Text.Json.Nodes;

namespace Nirvana.Public.Entities.Nirvana;

public class EntityUpdateConfig1 {
    
    public string Name = "Resource";
    public bool Safe = false;
    public required JsonArray Array;

    public int Count()
    {
        return Array.Count;
    }

}