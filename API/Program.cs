using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using TMS_API.BackgroundServices;
using TMS_API.Hubs;
using TMS_API.Listeners;
using TMS_API.Utilities;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    DotNetEnv.Env.Load(Path.Combine(Environment.CurrentDirectory, "Resources/.env"));
#endif

builder.Configuration.AddJsonFile("appsettings.json", optional: false).AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5188";
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "api1",
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:5188",
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
builder.Services.AddSingleton<ILinesListener, LinesListener>();
builder.Services.AddSingleton<SqlDependency>();
builder.Services.AddSingleton<IDatabaseActions, DatabaseActions>();

// Background Services
builder.Services.AddHostedService<HubsBackgroundService>();

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
