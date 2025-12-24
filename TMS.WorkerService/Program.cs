using TMS.Core.Extensions;
using TMS.WorkerService.BackgroundServices;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

builder.Services.AddLogging();
builder.Services.AddHostedService<LinesWorker>();

builder.Services.AddKafkaService();
builder.Services.AddHubConnectionFactory();

IHost host = builder.Build();

await host.RunAsync();
