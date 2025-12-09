using Microsoft.Extensions.DependencyInjection;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.Persistence;
using TMS.Persistence.Factories;

namespace TMS.Persistence.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension (IServiceCollection services)
        {
            public IServiceCollection AddUnitOfWork() => services.AddScoped<IUnitofWork, UnitofWork>();
            public IServiceCollection AddSqlConnectionFactory() => services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        }
    }
}