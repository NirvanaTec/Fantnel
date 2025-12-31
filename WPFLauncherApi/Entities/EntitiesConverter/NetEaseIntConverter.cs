using System.Text.Json;
using System.Text.Json.Serialization;

namespace WPFLauncherApi.Entities.EntitiesConverter;

public class NetEaseIntConverter : JsonConverter<string>
{
    public override string Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType != JsonTokenType.Number
            ? reader.GetString() ?? string.Empty
            : reader.GetInt32().ToString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}