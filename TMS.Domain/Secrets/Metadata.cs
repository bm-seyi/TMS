using System.Text.Json.Serialization;
using TMS.Domain.JsonConverters;

namespace TMS.Domain.Secrets
{
    public sealed record Metadata
    {
        [JsonPropertyName("created_time")]
        [JsonConverter(typeof(DateTimeConverter))]
        public required DateTime CreatedTime { get; init; }

        [JsonPropertyName("custom_metadata")]
        public string? CustomMetadata { get; init; }

        [JsonPropertyName("deletion_time")]
        [JsonConverter(typeof(DateTimeNullableConverter))]
        public DateTime? DeletionTime { get; init; }

        [JsonPropertyName("destroyed")]
        public bool Destroyed { get; init; }

        [JsonPropertyName("version")]
        public int Version { get; init; }
    }
}