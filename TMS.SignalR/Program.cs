using StackExchange.Redis;
using TMS.Core.Extensions;
using TMS.Core.Mapping;
using TMS.Core.Queries;
using TMS.Infrastructure.Extensions;
using TMS.SignalR.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

string redisPassword = builder.Configuration.GetRequiredValue<string>("Parameters:RedisPassword");
string redisEndpoint = builder.Configuration.GetRequiredValue<string>("Redis:Endpoint");

builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.AbortOnConnectFail = false;
        options.Configuration.Ssl = true;
        options.Configuration.Password = redisPassword;
        options.Configuration.EndPoints.Add(redisEndpoint);
        options.Configuration.ChannelPrefix = RedisChannel.Literal("TMS");
    });

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(LinesDataQuery).Assembly));
builder.Services.AddConnectionBehaviour();
builder.Services.AddTransactionBehaviour();

// AutoMapper
builder.Services.AddAutoMapper(crg => {}, typeof(AutoMapperProfile));

// Other Services
builder.Services.AddSqlConnectionFactory();
builder.Services.AddSqlSession();
builder.Services.AddHealthCheckProcedures();
builder.Services.AddLinesProcedures();
builder.Services.AddSecretService();
builder.Services.AddVaultClient(builder.Configuration);

WebApplication app = builder.Build();

app.MapHub<LinesDataHub>("/linesDataHub");

await app.RunAsync();
