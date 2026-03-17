namespace TMS.Domain.Configuration
{
    public sealed record KafkaOptions
    {
        public required string BootstrapServers { get; init; }
        public required string Topic { get; init; }
        public required string GroupId { get; init; }
    };
}