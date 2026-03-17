using System.Diagnostics;
using Microsoft.Extensions.Options;
using TMS.Application.Interfaces.Infrastructure.Messaging;
using TMS.Domain.Configuration;


namespace TMS.WorkerService.BackgroundServices
{
    internal sealed class LinesWorker(ILogger<LinesWorker> logger, IEventSubscriber eventSubscriber, IOptions<KafkaOptions> options) : BackgroundService
    {
        private readonly ILogger<LinesWorker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IEventSubscriber _eventSubscriber = eventSubscriber ?? throw new ArgumentNullException(nameof(eventSubscriber));
        private readonly KafkaOptions _kafkaOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.WorkerService");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = _activitySource.StartActivity("LinesWorker.ExecuteAsync");
            
            _logger.LogInformation("Starting Kafka event subscription for topic '{Topic}' on bootstrap servers '{BootstrapServers}' with group '{GroupId}'.", _kafkaOptions.Topic, _kafkaOptions.BootstrapServers, _kafkaOptions.GroupId);

            await _eventSubscriber.StartAsync(_kafkaOptions.Topic, _kafkaOptions.BootstrapServers, _kafkaOptions.GroupId, stoppingToken);
        }
    }
}