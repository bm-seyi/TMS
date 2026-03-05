namespace TMS.Domain.RateLimit
{
    public record TokenBucket
    {
        public int TokenLimit { get; init; }
        public int TokensPerPeriod { get; init; }
        public int ReplenishmentPeriodSeconds { get; init; }
        public int QueueLimit { get; init; }
        public bool AutoReplenishment { get; init; }
    }
}
