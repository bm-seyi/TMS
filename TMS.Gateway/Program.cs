using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TMS.Gateway.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry()
    .WithMetrics(x => x.AddAspNetCoreInstrumentation())
    .WithTracing(x =>  x.AddAspNetCoreInstrumentation(tracing => tracing.Filter = context => !context.Request.Path.StartsWithSegments("/health")));


// Authentication
builder.Services.AddAuthentication("Microsoft")
    .AddMicrosoftIdentityWebApi(builder.Configuration, jwtBearerScheme: "Microsoft");

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", x =>
    {
        x.RequireAuthenticatedUser();
    });
});

builder.Services.AddHealthChecks()
    .AddResourceUtilizationHealthCheck(o =>
    {
        o.CpuThresholds = new ResourceUsageThresholds
        {
        DegradedUtilizationPercentage = 80,
        UnhealthyUtilizationPercentage = 90,
        };
 
        o.MemoryThresholds = new ResourceUsageThresholds
        {
        DegradedUtilizationPercentage = 80,
        UnhealthyUtilizationPercentage = 90
        };
    });

builder.Services.AddConfiguredRateLimiting(builder.Configuration);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        handler.MaxConnectionsPerServer = 512;
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
        handler.EnableMultipleHttp2Connections = true;
    });

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
 
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsJsonAsync(report);
    }
});
app.MapReverseProxy();

await app.RunAsync();
