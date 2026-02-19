using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Application.Extensions;
using TMS.Application.Interfaces.Factories;
using TMS.Application.Interfaces.Infrastructure.Http;
using TMS.Application.Interfaces.Infrastructure.HubClients;
using TMS.Application.Interfaces.Infrastructure.Messaging;
using TMS.Application.Interfaces.Infrastructure.Persistence;
using TMS.Application.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Infrastructure.Factories;
using TMS.Infrastructure.Http;
using TMS.Infrastructure.HubClients;
using TMS.Infrastructure.Messaging;
using TMS.Infrastructure.Persistence;
using TMS.Infrastructure.Persistence.Procedures;


namespace TMS.Infrastructure.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension (IServiceCollection services)
        {
            public IServiceCollection AddSqlSession() => services.AddScoped<ISqlSession, SqlSession>();
            public IServiceCollection AddHealthCheckProcedures() => services.AddScoped<IHealthCheckProcedures, HealthCheckProcedures>();
            public IServiceCollection AddLinesProcedures() => services.AddScoped<ILinesProcedures, LinesProcedures>();
            public IServiceCollection AddSqlConnectionFactory() => services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            public IServiceCollection AddVaultClient(IConfiguration configuration)
            {
                services.AddHttpClient<IVaultClient, VaultClient>(x =>
                {
                    x.BaseAddress = new Uri(configuration.GetRequiredValue<string>("Vault:BaseUrl"));
                    x.DefaultRequestHeaders.Add("X-Vault-Token", configuration.GetRequiredValue<string>("Vault:Token"));
                });

                return services;
            }
            public IServiceCollection AddKafkaLinesEventSubscriber() => services.AddTransient<IEventSubscriber, KafkaLinesEventSubscriber>();
            public IServiceCollection AddKafkaService() => services.AddSingleton<IKafkaService, KafkaService>();

            public IServiceCollection AddLinesDataHub(IConfiguration configuration)
            {
                services.AddKeyedSingleton("LinesDataHub", (sp, key) =>
                {
                    string url = configuration.GetRequiredValue<string>("Hubs:LinesDataHub:Url");

                    return new HubConnectionBuilder()
                        .WithUrl(url)
                        .WithAutomaticReconnect()
                        .Build();
                });

                return services.AddSingleton<ILinesDataHubClient, LinesDataHubClient>();
            }
        }
    }
}