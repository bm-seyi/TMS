namespace TMS.Domain.RateLimit
{
    public record FixedWindow
    {
        public int PermitLimit { get; init; }
        public int WindowSeconds { get; init; }
        public int QueueLimit { get; init; }
    }
}