using System.Text.Json;

namespace Codexus.Development.SDK.Utils;

public static class JsonElementExtensions {
    public static object? GetObject(this JsonElement element)
    {
        return element.ValueKind switch {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetNumber(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.DeserializeToList(),
            JsonValueKind.Object => element.DeserializeToDictionary(),
            JsonValueKind.Null => null,
            _ => element
        };
    }

    private static object GetNumber(this JsonElement element)
    {
        if (element.TryGetInt32(out var value)) {
            return value;
        }

        if (element.TryGetInt64(out var value2)) {
            return value2;
        }

        if (element.TryGetDouble(out var value3)) {
            return value3;
        }

        if (element.TryGetDecimal(out var value4)) {
            return value4;
        }

        return element.ToString();
    }

    private static List<object?> DeserializeToList(this JsonElement element)
    {
        return element.EnumerateArray().Select(item => item.GetObject()).ToList();
    }

    private static Dictionary<string, object?> DeserializeToDictionary(this JsonElement element)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var item in element.EnumerateObject()) {
            dictionary[item.Name] = item.Value.GetObject();
        }

        return dictionary;
    }
}