using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StackExchange.Redis;
using TMS_API.Models.Configuration;
using TMS_API.Hubs;
using TMS_API.Utilities;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    string parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? throw new ArgumentNullException(nameof(parentDirectory));
    DotNetEnv.Env.Load(Path.Combine(parentDirectory, ".env"));
#endif

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();


builder.Services.AddSignalR().AddStackExchangeRedis(options =>
{
    RedisOptions redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>() ?? throw new InvalidOperationException("Redis configuration is missing");

    options.Configuration = new ConfigurationOptions
    {
        EndPoints = { redisOptions.Endpoint },
        AbortOnConnectFail = redisOptions.AbortOnConnectFail,
        ClientName = redisOptions.ClientName,
        Password = redisOptions.Password,
        IncludeDetailInExceptions = redisOptions.IncludeDetailInExceptions,
        IncludePerformanceCountersInExceptions = redisOptions.IncludePerformanceCountersInExceptions,
        ReconnectRetryPolicy = new LinearRetry(5000),
        ConnectTimeout = redisOptions.ConnectTimeout,
        SyncTimeout = redisOptions.SyncTimeout,
        AsyncTimeout = redisOptions.AsyncTimeout,
        ConnectRetry = redisOptions.ConnectRetry,
        KeepAlive = redisOptions.KeepAlive,
        ResolveDns = redisOptions.ResolveDns,
        CheckCertificateRevocation = redisOptions.CheckCertificateRevocation,
        AllowAdmin = redisOptions.AllowAdmin,
    };
});


builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    JwtOptions jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing");

    options.Authority = jwtOptions.Authority;
    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = jwtOptions.TokenValidationParameters.ValidateAudience,
        ValidAudience = jwtOptions.TokenValidationParameters.ValidAudience,
        ValidateIssuer = jwtOptions.TokenValidationParameters.ValidateIssuer,
        ValidIssuer = jwtOptions.TokenValidationParameters.ValidIssuer,
        ValidateIssuerSigningKey = jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey,
        ValidateLifetime = jwtOptions.TokenValidationParameters.ValidateLifetime,
        ClockSkew = TimeSpan.FromSeconds(jwtOptions.TokenValidationParameters.ClockSkew)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1. First try headers (HTTP requests)
            if (context.Request.Headers.TryGetValue("Authorization", out var headerToken))
            {
                context.Token = headerToken.ToString().Replace("Bearer ", "");
            }
            // 2. Fallback to query string (WebSockets)
            else if (context.Request.Query.TryGetValue("access_token", out var queryToken))
            {
                context.Token = queryToken;
            }
            return Task.CompletedTask;
        }
    };

    options.IncludeErrorDetails = builder.Environment.IsDevelopment();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("signalR.Read", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "signalR.read");
    });

    options.AddPolicy("signalR.Write", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "signalR.write");
    });
    
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
builder.Services.AddScoped<IDatabaseActions, DatabaseActions>();
builder.Services.AddScoped<IDatabaseConnection, DatabaseConnection>();

// Background Services
builder.Services.AddHostedService<LinesBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<LinesHub>("/LinesHub");

app.Run();
