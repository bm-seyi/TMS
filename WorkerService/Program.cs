using Core.Interfaces.Factories;
using Core.Services;
using WorkerService.BackgroundServices;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging();
builder.Services.AddSignalR();
builder.Services.AddHostedService<LinesWorker>();

builder.Services.AddSingleton<IKafkaService, KafkaService>();

IHost host = builder.Build();

await host.RunAsync();
