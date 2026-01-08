using System.Text.Json.Serialization;

namespace TMS.Models.Secrets
{
    public sealed record VaultData<T>
    {
        [JsonPropertyName("data")]
        public required T Data { get; init; }

        [JsonPropertyName("metadata")]
        public required Metadata Metadata { get; init; }
    }
}