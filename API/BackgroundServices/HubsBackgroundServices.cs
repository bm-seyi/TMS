
using Microsoft.Data.SqlClient;
using TMS_API.Listeners;

namespace TMS_API.BackgroundServices
{
    public class HubsBackgroundService : BackgroundService
    {
        private readonly ILinesListener _linesListener;
        private readonly ILogger<HubsBackgroundService> _logger;
        private readonly string _connectionString;

        public HubsBackgroundService(ILinesListener linesListener, ILogger<HubsBackgroundService> logger, IConfiguration configuration)
        {
            _linesListener = linesListener ?? throw new ArgumentNullException(nameof(linesListener));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString =  configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HubsBackgroundService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("HubsBackgroundService is stopping."));

            try
            {
                SqlDependency.Start(_connectionString);
                _logger.LogInformation("SqlDependency has started.");

                await _linesListener.StartListening(); 
                _logger.LogInformation("LinesListener has started.");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in HubsBackgroundService.");
            }
            finally
            {
                _linesListener.StopListening();
                _logger.LogInformation("LinesListener has stopped.");

                SqlDependency.Stop(_connectionString);
                _logger.LogInformation("SqlDependency has stopped.");
                
                _logger.LogInformation("HubsBackgroundService has stopped.");
            }

           
        }
    }
}