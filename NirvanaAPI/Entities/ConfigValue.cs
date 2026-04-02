using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NirvanaAPI.Entities;

public class ConfigValue {

    public required string Name;
    private string? _value;
    public required string Default;
    public string PropertyName = "string";

    public void SetValue(string? value)
    {
        _value = value;
    }

    public string GetValue()
    {
        return _value ?? Default;
    }
    
    public bool IsProperty(string propertyName)
    {
        return PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase);
    }

}