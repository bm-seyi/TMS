using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Core.Behaviours;
using TMS.Core.Factories;
using TMS.Core.HttpClients;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.HttpClients;
using TMS.Core.Interfaces.Services;
using TMS.Core.Services;

namespace TMS.Core.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddHubConnectionFactory() => services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
            public IServiceCollection AddConnectionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConnectionBehaviour<,>));
            public IServiceCollection AddTransactionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
            public IServiceCollection AddKafkaService() => services.AddSingleton<IKafkaService, KafkaService>();
            public IServiceCollection AddVaultClient(IConfiguration configuration)
            {
                services.AddHttpClient<IVaultClient, VaultClient>(x =>
                {
                    x.BaseAddress = new Uri(configuration.GetRequiredValue<string>("Vault:BaseUrl"));
                    x.DefaultRequestHeaders.Add("X-Vault-Token", configuration.GetRequiredValue<string>("Vault:Token"));
                });

                return services;
            }

            public IServiceCollection AddSecretService() => services.AddScoped<ISecretService, SecretsService>();
        }
    }
}