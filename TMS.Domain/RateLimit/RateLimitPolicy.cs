
namespace TMS.Domain.RateLimit
{
    public record RateLimitPolicy
    {
        public required string Type { get; init; }
        public TokenBucket? TokenBucket { get; init; }
        public FixedWindow? FixedWindow { get; init; }
    }
}
