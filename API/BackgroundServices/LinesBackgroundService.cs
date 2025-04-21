using Microsoft.AspNetCore.SignalR;
using TMS_API.Hubs;
using TMS_API.Models.Data;
using TMS_API.Utilities;

public class LinesBackgroundService : BackgroundService
{
    private readonly IHubContext<LinesHub> _hubContext;
    private readonly ILogger<LinesBackgroundService>_logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LinesBackgroundService(IHubContext<LinesHub> hubContext, ILogger<LinesBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("LinesBackgroundService is stopping."));
        _logger.LogInformation("LinesBackgroundService is starting.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {   
                await using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    IDatabaseActions _databaseActions = scope.ServiceProvider.GetRequiredService<IDatabaseActions>();
                    List<LinesQueueModel> queueData = await _databaseActions.RetrieveModelAsync<LinesQueueModel>("ProcessDataChangeQueueMessage", true, stoppingToken);

                    if (queueData.Count == 0)
                    {
                        _logger.LogInformation("No data in queue. Waiting for new data...");
                        await Task.Delay(500, stoppingToken);
                        continue;
                    }

                    await _hubContext.Clients.All.SendAsync("ReceiveLinesData", queueData, stoppingToken);
                    _logger.LogInformation("Lines data sent to all clients.");
                   
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is expected when the service is stopping, so we can ignore it.
                _logger.LogWarning("LinesBackgroundService is stopping due to cancellation.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Service Broker listener: {ex.Message}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
