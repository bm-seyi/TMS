using System.Threading.RateLimiting;
using System.Security.Claims;
using TMS.Domain.RateLimit;


namespace TMS.Gateway.RateLimit
{
    public static class RateLimiterPolicyFactory
    {
        public static Func<HttpContext, RateLimitPartition<string>> Create(RateLimitPolicy config)
        {
            return context =>
            {
                string key = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

                return config.Type switch
                {
                    "TokenBucket" =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            key,
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = config.TokenBucket!.TokenLimit,
                                TokensPerPeriod = config.TokenBucket.TokensPerPeriod,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(config.TokenBucket.ReplenishmentPeriodSeconds),
                                QueueLimit = config.TokenBucket.QueueLimit,
                                AutoReplenishment = config.TokenBucket.AutoReplenishment
                            }),

                    "FixedWindow" =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            key,
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = config.FixedWindow!.PermitLimit,
                                Window = TimeSpan.FromSeconds(config.FixedWindow.WindowSeconds),
                                QueueLimit = config.FixedWindow.QueueLimit
                            }),

                    _ => throw new NotSupportedException($"Unsupported limiter type: {config.Type}")
                };
            };
        }
    }
}
