using Microsoft.Extensions.DependencyInjection;
using TMS.Core.Factories;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Services;

namespace TMS.Core.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddHubConnectionFactory() => services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
            public IServiceCollection AddKafkaService() => services.AddSingleton<IKafkaService, KafkaService>();
        }
    }
}