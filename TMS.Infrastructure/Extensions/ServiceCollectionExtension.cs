using Microsoft.Extensions.DependencyInjection;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.Persistence;
using TMS.Core.Interfaces.Persistence.Procedures;
using TMS.Persistence.Factories;
using TMS.Persistence.Procedures;

namespace TMS.Persistence.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension (IServiceCollection services)
        {
            public IServiceCollection AddSqlSession() => services.AddScoped<ISqlSession, SqlSession>();
            public IServiceCollection AddHealthCheckProcedures() => services.AddScoped<IHealthCheckProcedures, HealthCheckProcedures>();
            public IServiceCollection AddLinesProcedures() => services.AddScoped<ILinesProcedures, LinesProcedures>();
            public IServiceCollection AddSqlConnectionFactory() => services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        }
    }
}