using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Core.Extensions;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.Infrastructure.Http;
using TMS.Core.Interfaces.Infrastructure.Persistence;
using TMS.Core.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Infrastructure.Factories;
using TMS.Infrastructure.Http;
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
        }
    }
}