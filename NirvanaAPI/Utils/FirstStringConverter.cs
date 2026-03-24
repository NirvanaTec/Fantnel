using System.Text.Json;
using System.Text.Json.Serialization;

namespace NirvanaAPI.Utils;

public class FirstStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.StartArray => ReadFirstElement(ref reader),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    private static string? ReadFirstElement(ref Utf8JsonReader reader)
    {
        string? result = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (result == null && reader.TokenType == JsonTokenType.String)
            {
                result = reader.GetString();
            }
            else
            {
                reader.Skip(); // 跳过非string或后续元素（含嵌套对象/数组）
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}