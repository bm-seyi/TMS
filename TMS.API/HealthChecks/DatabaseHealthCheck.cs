using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TMS.Application.Queries;
using TMS.Domain.DTOs;


namespace TMS.API.HealthChecks
{
    /// <summary>
    /// Represents a health check implementation for verifying the status of the SQL Server database.
    /// </summary>
    /// <remarks>
    /// This health check queries the database status and returns a result indicating whether the database is online.
    /// </remarks>
    public sealed class DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger, IMediator mediator) : IHealthCheck
    {
        private readonly ILogger<DatabaseHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API");

        /// <summary>
        /// Asynchronously checks the health of the SQL Server database by verifying its online status.
        /// </summary>
        /// <param name="context">The context in which the health check is being performed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="HealthCheckResult"/>
        /// indicating whether the database is healthy or unhealthy.
        /// </returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("DatabaseHealthCheck.CheckHealthAsync");
           
            try
            {
                DatabaseHealthCheckDTO databaseHealthCheckDTO = await _mediator.Send(new DatabaseHealthCheckQuery(), cancellationToken);
                if (databaseHealthCheckDTO.Status != "ONLINE")
                {
                    _logger.LogWarning("Database health check failed: status is '{Status}' instead of 'ONLINE'.", databaseHealthCheckDTO.Status);
                    return HealthCheckResult.Unhealthy($"Database is not online. Current status:{databaseHealthCheckDTO.Status}");
                }
                _logger.LogDebug("Database is online and operational");
                return HealthCheckResult.Healthy("Database is online and operational");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "The request was cancelled by the client.");
                return HealthCheckResult.Degraded("Health check was cancelled before completion.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during the database health check.");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}