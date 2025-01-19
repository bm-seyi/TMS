using TMS_API.Listeners;

namespace TMS_API.BackgroundServices
{
    public class HubsBackgroundService : BackgroundService
    {
        private readonly ILinesListener _linesListener;
        private readonly ILogger<HubsBackgroundService> _logger;
        private readonly ISqlDependencyManager _sqlDependencyManager;
        private readonly string _connectionString;

        public HubsBackgroundService(ILinesListener linesListener, ILogger<HubsBackgroundService> logger, IConfiguration configuration, ISqlDependencyManager sqlDependencyManager)
        {
            _linesListener = linesListener ?? throw new ArgumentNullException(nameof(linesListener));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _connectionString =  configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException (nameof(_connectionString));
            _sqlDependencyManager = sqlDependencyManager ?? throw new ArgumentNullException(nameof(sqlDependencyManager));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HubsBackgroundService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("HubsBackgroundService is stopping."));

            try
            {
                _sqlDependencyManager.Start(_connectionString);
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
                await _linesListener.StopListeningAsync();
                _logger.LogInformation("LinesListener has stopped.");

                _sqlDependencyManager.Stop(_connectionString);
                _logger.LogInformation("SqlDependency has stopped.");
                
                _logger.LogInformation("HubsBackgroundService has stopped.");
            }
        }
    }
}