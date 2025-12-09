using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Data;
using Core.Interfaces.Persistence;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.Persistence;


namespace TMS.Persistence
{
    /// <summary>
    /// Responsible for Database Actions.
    /// </summary>
    internal sealed class UnitofWork : IUnitofWork, IAsyncDisposable
    {
        private readonly ILogger<UnitofWork> _logger;
        private readonly Lazy<SqlConnection> _lazyConnection;
        internal DbTransaction? sqlTransaction;
        private static readonly ActivitySource _activitySource = new ActivitySource("FlightSearchEngine.Persistence.UnitofWork");
 
        public UnitofWork(ILogger<UnitofWork> logger, ISqlConnectionFactory sqlConnectionFactory)
        {
            _ = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
 
            _lazyConnection = new Lazy<SqlConnection>(() => sqlConnectionFactory.CreateConnection("DefaultConnection"));
        }
 
        private bool Disposed { get; set; } = false;
 
        internal SqlConnection sqlConnection => _lazyConnection.Value;
 
        private IProcedures? _procedures;
        public IProcedures Procedures => _procedures ??= new Procedures(this);
 
        /// <summary>
        /// Asynchronously opens the SQL database connection.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Logs a debug message upon successful connection.
        /// </remarks>
        internal async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.OpenAsync");
 
            ThrowIfConnectionDisposed();
 
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync(cancellationToken);
 
                _logger.LogDebug("SQL connection opened. Connection ID: {ConnectionId}", sqlConnection.ClientConnectionId);
            }
        }
 
        /// <summary>
        /// Opens a database connection and begins a transaction.
        /// </summary>
        /// <returns>Returns Task</returns>
        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.BeginAsync");
 
            ThrowIfConnectionDisposed();
 
            await OpenAsync(cancellationToken);
 
            if (sqlTransaction != null)
                throw new InvalidOperationException("Transaction already started.");
 
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
 
            ThrowIfConnectionDisposed();
 
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
 
            ThrowIfConnectionDisposed();
 
            if (sqlTransaction is null)
                throw new InvalidOperationException("Transaction has not been started.");
 
            try
            {
                await sqlTransaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back.");
            }
            finally
            {
                await sqlTransaction.DisposeAsync();
                sqlTransaction = null;
            }
        }
 
        /// <summary>
        /// Disposes of the transaction and connection.
        /// </summary>
        /// <returns>Returns ValueTask</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask DisposeAsync()
        {
            using Activity? activity = _activitySource.StartActivity("UnitofWork.DisposeAsync");
 
            if (Disposed) return;
 
            if (_lazyConnection.IsValueCreated)
            {
                if (sqlTransaction != null)
                {
                    try
                    {
                        await sqlTransaction.RollbackAsync();
                        _logger.LogWarning("Transaction rolled back automatically during dispose.");
                    }
                    finally
                    {
                        await sqlTransaction.DisposeAsync();
                        _logger.LogInformation("The Transaction was disposed");
                        sqlTransaction = null;
                    }
                }
 
                if (sqlConnection.State != ConnectionState.Closed)
                {
                    await sqlConnection.CloseAsync();
                    _logger.LogInformation("The SQL Connection was closed");
                }
 
                await sqlConnection.DisposeAsync();
                _logger.LogInformation("The SQL Connection was disposed");
            }
 
            Disposed = true;
 
            GC.SuppressFinalize(this);
        }
 
        private void ThrowIfConnectionDisposed() => ObjectDisposedException.ThrowIf(Disposed, nameof(UnitofWork));
    }
}
 