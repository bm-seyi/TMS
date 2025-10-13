using Core.Factories;
using Core.Interfaces.Factories;
using Core.Services;
using WorkerService.BackgroundServices;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

builder.Services.AddLogging();
builder.Services.AddHostedService<LinesWorker>();

builder.Services.AddSingleton<IKafkaService, KafkaService>();
builder.Services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();

IHost host = builder.Build();

await host.RunAsync();
