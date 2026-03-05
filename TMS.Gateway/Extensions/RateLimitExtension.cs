using System.Threading.RateLimiting;
using TMS.Domain.RateLimit;
using TMS.Gateway.RateLimit;

namespace TMS.Gateway.Extensions
{
    public static class RateLimiterExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddConfiguredRateLimiting(IConfiguration configuration)
            {
                Dictionary<string, RateLimitPolicy>? policies = configuration
                    .GetSection("RateLimiting:Policies")
                    .Get<Dictionary<string, RateLimitPolicy>>();

                if (policies is null)
                    return services;

                services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    foreach (var (name, config) in policies)
                    {
                        options.AddPolicy(name,
                            RateLimiterPolicyFactory.Create(config));
                    }

                    options.OnRejected = async (context, token) =>
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter,out var retryAfter))
                        {
                            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                        }

                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "rate_limit_exceeded",
                            message = "Too many requests."
                        }, token);
                    };
                });

                return services;
            }
        }
    }
}
