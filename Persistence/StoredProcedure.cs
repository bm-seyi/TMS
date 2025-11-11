using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Dapper;
using System.Diagnostics;
using Core.Interfaces.Persistence;
using Models.Dtos;


namespace QuoteService.Persistence
{
    public sealed class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly SqlConnection _sqlConnection;
        private readonly DbTransaction? _dbTransaction;
        private static readonly ActivitySource _activitySource = new ActivitySource("Persistence.StoredProcedureExecutor");

        public StoredProcedureExecutor(SqlConnection sqlConnection, DbTransaction? dbTransaction)
        {
            _sqlConnection = sqlConnection;
            _dbTransaction = dbTransaction;
        }


        public async Task<DatabaseHealthCheckDto> Usp_DatabaseHealthCheckAsync(CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("StoredProcedureExecutor.Usp_DatabaseHealthCheckAsync");

            CommandDefinition commandDefinition = new CommandDefinition("usp_DatabaseHealthCheck", null, _dbTransaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return await _sqlConnection.QuerySingleAsync<DatabaseHealthCheckDto>(commandDefinition);
        }
    }
}