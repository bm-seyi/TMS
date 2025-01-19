using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using TMS_API.Hubs;
using TMS_API.Utilities;
using System.Data;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Data.Common;

namespace TMS_API.Listeners
{
    public interface ILinesListener
    {
        Task StartListening();
        Task StopListeningAsync();
    }

    public class LinesListener : ILinesListener
    {
        private ISqlDependencyManager _sqlDependencyManager; 
        private readonly IHubContext<LinesHub> _hubContext;
        private readonly string _connectionString;
        private readonly ILogger<LinesListener> _logger;
        private readonly IDatabaseActions _databaseActions;
        private readonly IDbConnection _dbConnection;

        private SqlConnection?  sqlConnection;
        private ISqlDependency? sqlDependency;
        private bool isListening = false;

        public LinesListener(ISqlDependencyManager sqlDependencyManager, IHubContext<LinesHub> hubContext, IConfiguration configuration, ILogger<LinesListener> logger, IDatabaseActions databaseActions, IDbConnection dbConnection)
        {
            _sqlDependencyManager = sqlDependencyManager ?? throw new ArgumentNullException(nameof(sqlDependencyManager));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _connectionString =  configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(_connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task StartListening()
        {
            string query = "SELECT * FROM [dbo].[Lines]";

            IDbCommand dbCommand = _dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            sqlDependency = _sqlDependencyManager.Create((SqlCommand)dbCommand);
            sqlDependency.OnChange += new OnChangeEventHandler(Dependency_OnChange);

            await ((DbConnection)_dbConnection).OpenAsync();
            await ((SqlCommand)dbCommand).ExecuteReaderAsync();
            await ((DbConnection)_dbConnection).CloseAsync();
                
        }

        private void Dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            try
            {
                // Call an asynchronous method to handle the change
                _ = HandleDependencyChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dependency_OnChange");
            }
        }

        private async Task HandleDependencyChangeAsync()
        {
            try
            {
                List<Dictionary<string, object>> data = await _databaseActions.RetrieveLinesData();
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dependency_OnChange");
            }
        }

        public async Task StopListeningAsync()
        {
            if (isListening)
            {
                if (sqlDependency != null)
                {
                    sqlDependency.OnChange -= Dependency_OnChange;
                    sqlDependency = null!;
                    _logger.LogInformation("SqlDependency has been successfully stopped.");
                }

                if (sqlConnection != null)
                {
                    await sqlConnection.CloseAsync(); 
                    _logger.LogInformation("SQL connection has been successfully Closed.");

                    await sqlConnection.DisposeAsync();
                    _logger.LogInformation("SQL connection has been successfully Disposed.");
                    sqlConnection = null!;
                }
            }
           
        } 
    }
}