using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using TMS_API.Hubs;
using TMS_API.Utilities;

namespace TMS_API.Listeners
{
    public interface ILinesListener
    {
        Task StartListening();
        void StopListening();
    }

    public class LinesListener : ILinesListener
    {
        private SqlDependency _dependency; 
        private readonly IHubContext<LinesHub> _hubContext;
        private readonly string _connectionString;
        private readonly ILogger<LinesListener> _logger;
        private readonly IDatabaseActions _databaseActions;

        public LinesListener(SqlDependency dependency, IHubContext<LinesHub> hubContext, IConfiguration configuration, ILogger<LinesListener> logger, IDatabaseActions databaseActions)
        {
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _connectionString =  configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
        }

        public async Task StartListening()
        {
            string query = "SELECT * FROM [dbo].[Lines]";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                _dependency = new SqlDependency(command);
                _dependency.OnChange += new OnChangeEventHandler(Dependency_OnChange);

                await connection.OpenAsync();
                await command.ExecuteReaderAsync();
                await connection.CloseAsync();
            }
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

        public void StopListening()
        {
            _dependency.OnChange -= Dependency_OnChange;
            _dependency = null!;
            _logger.LogInformation("SqlDependency has been successfully stopped.");

        } 
    }
}