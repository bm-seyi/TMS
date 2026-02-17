using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Data;
using TMS.Core.Interfaces.Factories;
using TMS.Core.Interfaces.Persistence;


namespace TMS.Infrastructure.Persistence
{
    internal sealed class SqlSession : ISqlSession, IAsyncDisposable
    {
        private readonly ILogger<SqlSession> _logger;
        private readonly Lazy<SqlConnection> _lazyConnection;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Persistence.SqlSession");
 
        public SqlSession(ILogger<SqlSession> logger, ISqlConnectionFactory sqlConnectionFactory)
        {
            ArgumentNullException.ThrowIfNull(sqlConnectionFactory);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lazyConnection = new Lazy<SqlConnection>(() => sqlConnectionFactory.CreateConnection("DefaultConnection"));
        }
        public DbTransaction? Transaction { get; private set; }
        public SqlConnection Connection => _lazyConnection.Value;
        private bool disposed = false;

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("SqlSession.OpenAsync");
 
            ThrowIfConnectionDisposed();
 
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync(cancellationToken);
 
                _logger.LogDebug("SQL connection opened. Connection ID: {ConnectionId}", Connection.ClientConnectionId);
            }
        }
 
        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("SqlSession.BeginAsync");
 
            ThrowIfConnectionDisposed();
 
            await OpenAsync(cancellationToken);
 
            if (Transaction != null)
                throw new InvalidOperationException("Transaction already started.");
 
            Transaction = await Connection.BeginTransactionAsync(cancellationToken);
 
            _logger.LogDebug("A transaction has been created.");
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("SqlSession.CommitAsync");
 
            ThrowIfConnectionDisposed();
 
            if (Transaction == null)
                throw new InvalidOperationException("Ensure 'BeginAsync' is called before 'CommitAsync' to allow a valid connection and transaction to be created.");
           
            try
            {
                await Transaction.CommitAsync(cancellationToken);
                _logger.LogDebug("Transaction committed.");
            }
            finally
            {
                await Transaction.DisposeAsync();
                Transaction = null;
            }
 
        }
 
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("SqlSession.RollbackAsync");
  
            if (Transaction is null)
                throw new InvalidOperationException("Transaction has not been started.");
 
            try
            {
                await Transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back.");
            }
            finally
            {
                await Transaction.DisposeAsync();
                _logger.LogInformation("The Transaction was disposed");
                Transaction = null;
            }
        }
 
        public async ValueTask DisposeAsync()
        {
            using Activity? activity = _activitySource.StartActivity("SqlSession.DisposeAsync");
 
            if (disposed) return;
 
            if (_lazyConnection.IsValueCreated)
            {
                if (Transaction != null)
                   await RollbackAsync();
                
                if (Connection.State != ConnectionState.Closed)
                {
                    await Connection.CloseAsync();
                    _logger.LogInformation("The SQL Connection was closed");
                }
 
                await Connection.DisposeAsync();
                _logger.LogInformation("The SQL Connection was disposed");
            }
 
            disposed = true;
 
            GC.SuppressFinalize(this);
        }
 
        private void ThrowIfConnectionDisposed() => ObjectDisposedException.ThrowIf(disposed, nameof(SqlSession));
    }
}
 