using System.Data;
using System.Diagnostics;
using Dapper;
using Models.Dtos;
using TMS.Core.Interfaces.Persistence;

namespace TMS.Persistence
{
    /// <summary>
    /// Provides methods for executing and querying stored procedures using Dapper with support for transactions.
    /// </summary>
    internal sealed class Procedures : IProcedures
    {
        private readonly UnitofWork _unitofWork;
        private static readonly ActivitySource _activitySource = new ActivitySource("FlightSearchEngine.Persistence.Procedures");
 
        public Procedures(UnitofWork unitofWork)
        {
            _unitofWork = unitofWork ?? throw new ArgumentNullException(nameof(unitofWork));
        }
 

        public async Task<DatabaseHealthCheckDto> Usp_DatabaseHealthCheckAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("Procedures.Usp_DatabaseHealthCheckAsync");
            await _unitofWork.OpenAsync();
 
            CommandDefinition commandDefinition = new CommandDefinition("usp_DatabaseHealthCheck", null, _unitofWork.sqlTransaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return await _unitofWork.sqlConnection.QuerySingleAsync<DatabaseHealthCheckDto>(commandDefinition);
        }
    }
}