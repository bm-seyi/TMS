using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TMS.API.ExceptionHandlers;
using TMS.API.HealthChecks;
using TMS.API.Middleware;
using TMS.Core.Extensions;
using TMS.Core.Queries;
using TMS.Persistence.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<OperationCanceledHandler>();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DatabaseHealthCheckQuery).Assembly));
builder.Services.AddConnectionBehaviour();
builder.Services.AddTransactionBehaviour();


builder.Services.AddSqlSession();
builder.Services.AddHealthCheckProcedures();
builder.Services.AddSqlConnectionFactory();
builder.Services.AddLinesProcedures();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<TraceMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
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

app.MapControllers();

await app.RunAsync();
