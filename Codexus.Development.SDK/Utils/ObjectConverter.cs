using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codexus.Development.SDK.Utils;

public class ObjectConverter : JsonConverter<object> {
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number when reader.TryGetInt32(out var value) => value,
            JsonTokenType.Number when reader.TryGetInt64(out var value2) => value2,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.Null => null,
            JsonTokenType.StartObject => JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options),
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<object>>(ref reader, options),
            _ => JsonSerializer.Deserialize<JsonElement>(ref reader).GetObject()
        };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
    }
}