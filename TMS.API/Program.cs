using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using TMS.API.ExceptionHandlers;
using TMS.API.HealthChecks;
using TMS.Application.Extensions;
using TMS.Application.Mapping;
using Asp.Versioning;
using TMS.Infrastructure.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// OpenTelemetry
builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry()
    .WithMetrics(x => x.AddAspNetCoreInstrumentation())
    .WithTracing(x =>  x.AddAspNetCoreInstrumentation(tracing => tracing.Filter = context => !context.Request.Path.StartsWithSegments("/health")));

// Exception Handler
builder.Services.AddExceptionHandler<OperationCanceledHandler>();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);

// Authorization
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.UnsupportedApiVersionStatusCode = StatusCodes.Status404NotFound;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { MediaTypeNames.Application.Json });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("DatabaseHealthCheck", failureStatus: HealthStatus.Unhealthy)
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


// Http Logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                            HttpLoggingFields.ResponsePropertiesAndHeaders;
});

// Output Cache
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
    options.AddBasePolicy(policy =>
    {
        policy.Cache();
        policy.Expire(TimeSpan.FromSeconds(60));
    });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// MediatR
builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly);});
builder.Services.AddDatabaseHealthCheckHandler();
builder.Services.AddGetArcgisSecretHandler();
builder.Services.AddConnectionBehaviour();
builder.Services.AddTransactionBehaviour();

// AutoMapper
builder.Services.AddAutoMapper(crg => {}, typeof(AutoMapperProfile));

// Other Services
builder.Services.AddSqlSession();
builder.Services.AddHealthCheckProcedures();
builder.Services.AddSqlConnectionFactory();
builder.Services.AddLinesProcedures();
builder.Services.AddSecretService();
builder.Services.AddVaultClient(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
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

app.MapControllers();

await app.RunAsync();
