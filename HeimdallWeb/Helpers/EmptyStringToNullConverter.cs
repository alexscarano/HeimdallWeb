using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeimdallWeb.Helpers
{
    public class EmptyStringToNullConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var str = reader.GetString();
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(value))
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }

}
