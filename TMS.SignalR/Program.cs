using SignalR.Hubs;
using StackExchange.Redis;
using TMS.Persistence.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

string redisPassword = builder.Configuration.GetValue<string>("Parameters:RedisPassword") ?? throw new InvalidOperationException("Redis Password is not configured.");
string redisEndpoint = builder.Configuration.GetValue<string>("Redis:Endpoint") ?? throw new InvalidOperationException("Redis endpoint is not configured.");

builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.AbortOnConnectFail = false;
        options.Configuration.Password = redisPassword;
        options.Configuration.EndPoints.Add(redisEndpoint);
        options.Configuration.ChannelPrefix = RedisChannel.Literal("TMS");
    });

builder.Services.AddSqlConnectionFactory();
builder.Services.AddUnitOfWork();

WebApplication app = builder.Build();

app.MapHub<LinesHub>("/linesHub");

await app.RunAsync();
