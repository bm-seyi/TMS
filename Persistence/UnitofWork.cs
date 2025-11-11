using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using Core.Interfaces.Persistence;
using Core.Interfaces.Factories;


namespace Persistence
{
    /// <summary>
    /// Responsible for Database Actions.
    /// </summary>
    public class UnitofWork : IUnitofWork
    {
        private readonly ILogger<UnitofWork> _logger;
        private readonly Lazy<SqlConnection> _lazyConnection;
        private DbTransaction? sqlTransaction;
        private static readonly ActivitySource _activitySource = new ActivitySource("Persistence.UnitofWork");
        public UnitofWork(ISqlDatabaseFactory sqlDatabaseFactory, ILogger<UnitofWork> logger)
        {
            _ = sqlDatabaseFactory ?? throw new ArgumentNullException(nameof(sqlDatabaseFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _lazyConnection = new Lazy<SqlConnection>(() => sqlDatabaseFactory.CreateConnection("DefaultConnection"));
        }

        private SqlConnection sqlConnection => _lazyConnection.Value;

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
            using Activity? activity = _activitySource.StartActivity("UnitofWork.OpenAsync");

            if (sqlConnection.State != System.Data.ConnectionState.Open)
            {
                await sqlConnection.OpenAsync(cancellationToken);
                _logger.LogDebug("Connection to database opened.");
            }
        }

        /// <summary>
        /// Opens a database connection and begins a transaction. 
        /// </summary>
        /// <returns>Returns Task</returns>
        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.BeginAsync");

            await OpenAsync(cancellationToken);

            sqlTransaction = await sqlConnection.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("A transaction has been created.");
        }

        /// <summary>
        /// Commits the transaction to the database.
        /// </summary>
        /// <returns>Returns Task</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.CommitAsync");

            if (sqlTransaction == null)
                throw new InvalidOperationException("Ensure 'BeginAsync' is called before 'CommitAsync' to allow a valid connection and transaction to be created.");
            
            try
            {
                await sqlTransaction.CommitAsync(cancellationToken);
                _logger.LogDebug("Transaction committed.");
            }
            catch (Exception ex)
            {
                await sqlTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(ex, "Error during commit; rolled back.");
            }
            finally
            {
                await sqlTransaction.DisposeAsync();
                sqlTransaction = null;
            }

        }

        /// <summary>
        /// Rolls the transaction back.
        /// </summary>
        /// <returns>Returns a Task</returns>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.RollbackAsync");

            if (sqlTransaction is null)
                throw new InvalidOperationException("Transaction has not been started.");

            await sqlTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Transaction rolled back.");
            await sqlTransaction.DisposeAsync();
            sqlTransaction = null;
        }

        /// <summary>
        /// Disposes of the transaction and connection.
        /// </summary>
        /// <returns>Returns ValueTask</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask DisposeAsync()
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.DisposeAsync");

            if (_lazyConnection.IsValueCreated)
            {
                if (sqlTransaction != null)
                {
                    await sqlTransaction.DisposeAsync();
                    _logger.LogInformation("The Transaction was disposed");
                }

                if (sqlConnection.State != System.Data.ConnectionState.Closed)
                {
                    await sqlConnection.CloseAsync();
                    _logger.LogInformation("The SQL Connection was closed");
                }

                await sqlConnection.DisposeAsync();
                _logger.LogInformation("The SQL Connection was disposed");
            }

            GC.SuppressFinalize(this);
        }
    }
}