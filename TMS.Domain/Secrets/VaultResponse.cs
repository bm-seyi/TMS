using System.Text.Json.Serialization;

namespace TMS.Domain.Secrets
{
    public sealed record VaultResponse<T>
    {
        [JsonPropertyName("request_id")]
        public required string RequestId { get; init;}

        [JsonPropertyName("lease_id")]
        public required string LeaseId { get; init; }

        [JsonPropertyName("renewable")]
        public required bool Renewable { get; init; }

        [JsonPropertyName("lease_duration")]
        public required int LeaseDuration { get; init; }

        [JsonPropertyName("data")]
        public required VaultData<T> Data { get; init; }

        [JsonPropertyName("wrap_info")]
        public string? WrapInfo { get; init; }

        [JsonPropertyName("warnings")]
        public string? Warnings { get; init; }

        [JsonPropertyName("auth")]
        public string? Auth { get; init; }

        [JsonPropertyName("mount_type")]
        public required string MountType { get; init; }
    }
}