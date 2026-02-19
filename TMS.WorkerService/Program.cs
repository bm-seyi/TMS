using TMS.Application.Extensions;
using TMS.Application.Queries;
using TMS.Infrastructure.Extensions;
using TMS.WorkerService.BackgroundServices;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

builder.Services.AddLogging();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DatabaseHealthCheckQuery).Assembly));
builder.Services.AddConnectionBehaviour();
builder.Services.AddTransactionBehaviour();

// Services
builder.Services.AddKafkaService();
builder.Services.AddKafkaLinesEventSubscriber();
builder.Services.AddLinesDataHub(builder.Configuration);

// Background Services
builder.Services.AddHostedService<LinesWorker>();


IHost host = builder.Build();

await host.RunAsync();
