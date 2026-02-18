using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TMS.Application.PipelineBehaviours;
using TMS.Application.Factories;
using TMS.Application.Interfaces.Factories;
using TMS.Application.Interfaces.Services;
using TMS.Application.Services;

namespace TMS.Application.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddHubConnectionFactory() => services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
            public IServiceCollection AddConnectionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConnectionBehaviour<,>));
            public IServiceCollection AddTransactionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
            public IServiceCollection AddKafkaService() => services.AddSingleton<IKafkaService, KafkaService>();
            public IServiceCollection AddSecretService() => services.AddScoped<ISecretService, SecretsService>();
        }
    }
}