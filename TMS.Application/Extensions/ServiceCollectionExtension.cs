using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TMS.Application.PipelineBehaviours;
using TMS.Application.Interfaces.Services;
using TMS.Application.Services;
using TMS.Application.Queries;
using TMS.Domain.DTOs;
using TMS.Application.Handlers;
using TMS.Domain.Secrets;


namespace TMS.Application.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddDatabaseHealthCheckHandler() => services.AddTransient<IRequestHandler<DatabaseHealthCheckQuery, DatabaseHealthCheckDTO>, DatabaseHealthCheckHandler>();
            public IServiceCollection AddGetArcgisSecretHandler() => services.AddTransient<IRequestHandler<GetArcgisSecretQuery, ArcgisSecret>, GetArcgisSecretHandler>();
            public IServiceCollection AddGetLinesDataHandler() => services.AddTransient<IRequestHandler<LinesDataQuery, IEnumerable<LinesReadDTO>>, LinesDataHandler>();
            public IServiceCollection AddConnectionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConnectionBehaviour<,>));
            public IServiceCollection AddTransactionBehaviour() => services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
            public IServiceCollection AddSecretService() => services.AddScoped<ISecretService, SecretsService>();
        }
    }
}