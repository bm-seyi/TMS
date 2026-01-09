using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TMS.Models.JsonConverters
{
    public sealed class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString()!;

            // Trim to 7 fractional digits
            s = Regex.Replace(s, @"\.(\d{7})\d+Z$", ".$1Z");

            return DateTime.Parse(
                s,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}

