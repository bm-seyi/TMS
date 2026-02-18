
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TMS.Domain.JsonConverters
{
    public sealed class DateTimeNullableConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var s = reader.GetString();

        if (string.IsNullOrWhiteSpace(s))
            return null;

        s = Regex.Replace(s, @"\.(\d{7})\d+Z$", ".$1Z");

        return DateTime.Parse(
            s,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString("O"));
    }
}
}