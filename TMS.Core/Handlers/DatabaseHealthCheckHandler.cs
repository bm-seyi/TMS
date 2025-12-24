using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.Persistence;
using TMS.Core.Queries;
using TMS.Models.DTOs;

namespace TMS.Core.Handlers
{
    public sealed class DatabaseHealthCheckHandler : IRequestHandler<DatabaseHealthCheckQuery, DatabaseHealthCheckDTO>
    {
        private readonly ILogger<DatabaseHealthCheckHandler> _logger;
        private readonly IHealthCheckProcedures _healthCheckProcedures;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Handlers.DatabaseHealthCheckHandler");

        public DatabaseHealthCheckHandler(ILogger<DatabaseHealthCheckHandler> logger, IHealthCheckProcedures healthCheckProcedures)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _healthCheckProcedures = healthCheckProcedures ?? throw new ArgumentNullException(nameof(healthCheckProcedures));
        }

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