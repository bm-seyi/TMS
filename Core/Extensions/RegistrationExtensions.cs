using Microsoft.Extensions.DependencyInjection;
using Core.Factories;
using Core.Interfaces.Factories;
using Core.Services;


namespace Core.Extensions
{
    public static class RegistrationExtensions
    {
        public static void AddHubConnectionFactory(this IServiceCollection services)
        {
            services.AddSingleton<IHubConnectionFactory, HubConnectionFactory>();
        }

        public static void AddSqlDatabaseFactory(this IServiceCollection services)
        {
            services.AddSingleton<ISqlDatabaseFactory, SqlDatabaseFactory>();
        }

        public static void AddKafkaService(this IServiceCollection services)
        {
            services.AddSingleton<IKafkaService, KafkaService>();
        }
    }
}