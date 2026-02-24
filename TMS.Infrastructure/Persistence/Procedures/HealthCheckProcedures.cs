using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Persistence;
using TMS.Application.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Domain.DTOs;

namespace TMS.Infrastructure.Persistence.Procedures
{
    internal sealed class HealthCheckProcedures : IHealthCheckProcedures
    {
        private readonly ILogger<HealthCheckProcedures> _logger;
        private readonly ISqlSession _sqlSession;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Infrastructure.Persistence.Procedures.HealthCheckProcedures");
 
        public HealthCheckProcedures(ILogger<HealthCheckProcedures> logger, ISqlSession sqlSession)
        {
            _logger =  logger ?? throw new ArgumentNullException(nameof(logger));
            _sqlSession = sqlSession ?? throw new ArgumentNullException(nameof(sqlSession));
        }

        public async Task<DatabaseHealthCheckDTO> DatabaseHealthCheckAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("HealthCheckProcedures.DatabaseHealthCheckAsync");

            _logger.LogDebug("Executing database health check procedure.");

            CommandDefinition commandDefinition = new CommandDefinition("usp_DatabaseHealthCheck", null, _sqlSession.Transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            DatabaseHealthCheckDTO result = await _sqlSession.Connection.QuerySingleAsync<DatabaseHealthCheckDTO>(commandDefinition);
            
            _logger.LogInformation("Database health check procedure executed successfully.");
            return result;
        }
    }
}