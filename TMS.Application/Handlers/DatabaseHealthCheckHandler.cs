using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Application.Queries;
using TMS.Domain.DTOs;


namespace TMS.Application.Handlers
{
    public sealed class DatabaseHealthCheckHandler(ILogger<DatabaseHealthCheckHandler> logger, IHealthCheckProcedures healthCheckProcedures) : IRequestHandler<DatabaseHealthCheckQuery, DatabaseHealthCheckDTO>
    {
        private readonly ILogger<DatabaseHealthCheckHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHealthCheckProcedures _healthCheckProcedures = healthCheckProcedures ?? throw new ArgumentNullException(nameof(healthCheckProcedures));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Application");

        public async Task<DatabaseHealthCheckDTO> Handle(DatabaseHealthCheckQuery request, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("DatabaseHealthCheckHandler.Handle");

            _logger.LogDebug("Handling DatabaseHealthCheckQuery.");

            DatabaseHealthCheckDTO databaseHealthCheckDTO =  await  _healthCheckProcedures.DatabaseHealthCheckAsync(cancellationToken);

            _logger.LogDebug("DatabaseHealthCheckQuery handled successfully.");
            return databaseHealthCheckDTO;
        }
    }
}