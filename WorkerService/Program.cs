using WorkerService.BackgroundServices;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSignalR();

IHost host = builder.Build();

await host.RunAsync();
