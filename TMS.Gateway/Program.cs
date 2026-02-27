using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration, jwtBearerScheme: "Microsoft");

// Authorization
builder.Services.AddAuthorization();

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

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

await app.RunAsync();
