using SignalR.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

app.MapHub<LinesHub>("/linesHub");

await app.RunAsync();
