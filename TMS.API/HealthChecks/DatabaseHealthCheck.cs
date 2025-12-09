using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TMS.Core.Interfaces.Persistence;
using TMS.Models.DTOs;


[assembly: InternalsVisibleTo("TMS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace TMS.API.HealthChecks
{
    /// <summary>
    /// Represents a health check implementation for verifying the status of the SQL Server database.
    /// </summary>
    /// <remarks>
    /// This health check queries the database status and returns a result indicating whether the database is online.
    /// </remarks>
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private readonly IUnitofWork _unitofWork;
        private static readonly ActivitySource _activitySource = new ActivitySource("QuoteService.Core.HealthChecks.DatabaseHealthCheck");
        public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger, IUnitofWork unitofWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitofWork = unitofWork ?? throw new ArgumentNullException(nameof(unitofWork));
        }
 
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
                DatabaseHealthCheckDTO databaseHealthCheck = await _unitofWork.Procedures.Usp_DatabaseHealthCheckAsync(cancellationToken);
 
                if (databaseHealthCheck.Status != "ONLINE")
                {
                    _logger.LogWarning("Database health check failed: status is '{Status}' instead of 'ONLINE'.", databaseHealthCheck.Status);
                    return HealthCheckResult.Unhealthy($"Database is not online. Current status:{databaseHealthCheck.Status}");
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