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
                    object? queueData = await _databaseActions.ProcessQueueMessageAsync(stoppingToken);
            
                    switch (queueData)
                    {
                        case List<LinesModel> lines:
                            await _hubContext.Clients.All.SendAsync("ReceiveLinesData", lines, stoppingToken);
                            _logger.LogInformation("Lines data sent to all clients.");
                            break;
                        
                        case List<Guid> affectedIds:
                            await _hubContext.Clients.All.SendAsync("DeletedLinesData", affectedIds, stoppingToken);
                            _logger.LogInformation("Deleted lines data sent to all clients.");
                            break;
                        
                        case null:
                            _logger.LogWarning("Received null data from queue.");
                            await Task.Delay(500, stoppingToken);
                            break;
                        
                        default:
                            _logger.LogWarning("Received unknown data type from queue: {1}", queueData?.GetType().Name);
                            break;
                    }
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
