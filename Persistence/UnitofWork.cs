using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Dapper;
using Core.Interfaces.Persistence;
using Core.Interfaces.Factories;
using Models.Dtos;



namespace FlightSearchEngine.Persistence
{
    /// <summary>
    /// Responsible for Database Actions.
    /// </summary>
    public class UnitofWork : IUnitofWork
    {
        private readonly ILogger<UnitofWork> _logger;
        private readonly SqlConnection _sqlConnection;
        private DbTransaction? sqlTransaction;

        public UnitofWork(ISqlDatabaseFactory sqlDatabaseFactory, ILogger<UnitofWork> logger)
        {
            _ = sqlDatabaseFactory ?? throw new ArgumentNullException(nameof(sqlDatabaseFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _sqlConnection = sqlDatabaseFactory.CreateConnection("DefaultConnection");
        }
    

        /// <summary>
        /// Asynchronously opens the SQL database connection.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Logs a debug message upon successful connection.
        /// </remarks>
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await _sqlConnection.OpenAsync(cancellationToken);
            _logger.LogDebug("Connection to database has successfully been opened.");
        }

        /// <summary>
        /// Opens a database connection and begins a transaction. 
        /// </summary>
        /// <returns>Returns Task</returns>
        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);

            sqlTransaction = await _sqlConnection.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("A transaction has successfully been created.");
        }

        /// <summary>
        /// Commits the transaction to the database.
        /// </summary>
        /// <returns>Returns Task</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (sqlTransaction == null)
                {
                    throw new InvalidOperationException("Ensure 'BeginAsync' is called before 'CommitAsync' to allow a valid connection and transaction to be created.");
                }

                try
                {
                    await sqlTransaction.CommitAsync(cancellationToken);
                    _logger.LogDebug("The Transaction has successfully been committed.");
                }
                catch (Exception ex)
                {
                    await sqlTransaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning(ex, "Error occurred during transaction commit.");
                }
                finally
                {
                    await sqlTransaction.DisposeAsync();
                    _logger.LogDebug("The Transaction has successfully been disposed.");
                }
            }
            finally
            {
                await CloseAsync();
            }

        }

        /// <summary>
        /// Rolls the transaction back.
        /// </summary>
        /// <returns>Returns a Task</returns>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (sqlTransaction == null)
                {
                    throw new InvalidOperationException("Ensure 'BeginAsync' is called before 'CommitAsync' to allow a valid connection and transaction to be created.");
                }

                await sqlTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("A Rollback occurred on the Transaction");
            }
            catch
            {
                await CloseAsync();
            }
        }

        /// <summary>
        /// Disposes of the transaction and connection.
        /// </summary>
        /// <returns>Returns ValueTask</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask DisposeAsync()
        {
            if (sqlTransaction != null)
            {
                await sqlTransaction.DisposeAsync();
                _logger.LogInformation("The Transaction was successfully disposed");
            }
            
            await CloseAsync();
        }
        
   
        /// <summary>
        /// Asynchronously performs a health check on the current SQL Server database by querying its name and status.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="DatabaseHealthCheckDto"/>
        /// with the database name and its current status.
        /// </returns>
        public async Task<DatabaseHealthCheckDto> HealthCheckAsync(CancellationToken cancellationToken = default)
        {
            string query = "SELECT [name] AS [DatabaseName], [state_desc] AS [Status] FROM sys.databases WHERE [name] = DB_NAME();";
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                await _sqlConnection.OpenAsync(cancellationToken);
            }

            CommandDefinition commandDefinition = new CommandDefinition(query, cancellationToken: cancellationToken);
            IEnumerable<DatabaseHealthCheckDto> healthCheckDtos = await _sqlConnection.QueryAsync<DatabaseHealthCheckDto>(commandDefinition);

            DatabaseHealthCheckDto databaseHealthCheck = healthCheckDtos.FirstOrDefault() ?? new DatabaseHealthCheckDto() { DatabaseName = _sqlConnection.Database, Status = "Unknown" };
            _logger.LogDebug("HEALTH CHECK --- Database: {0}, Status: {1}", databaseHealthCheck.DatabaseName, databaseHealthCheck.Status);

            return databaseHealthCheck;
        }

        /// <summary>
        /// A Helper Method to close and dispose Sql Connections
        /// </summary>
        /// <returns>Returns Task</returns>
        public async Task CloseAsync()
        {
            if (_sqlConnection.State != System.Data.ConnectionState.Closed)
            {
                await _sqlConnection.CloseAsync();
                await _sqlConnection.DisposeAsync();
                _logger.LogInformation("The Sql Connection has successfully been closed");
            }
        }
    }
}