using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using TMS.Application.Extensions;
using TMS.Application.Interfaces.Infrastructure.Messaging;


namespace TMS.WorkerService.BackgroundServices
{
    public sealed class LinesWorker : BackgroundService
    {
        private readonly ILogger<LinesWorker> _logger;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IConfiguration _configuration;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.WorkerService.BackgroundServices.LinesWorker");

        public LinesWorker(ILogger<LinesWorker> logger, IEventSubscriber eventSubscriber, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventSubscriber = eventSubscriber ?? throw new ArgumentNullException(nameof(eventSubscriber));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = _activitySource.StartActivity("LinesWorker.ExecuteAsync");

            const string topic = "sqlserver.TMS.dbo.Lines";
            const string groupId = "lines-consumer-group";

            string bootstrapServers = _configuration.GetRequiredValue<string>("Kafka:BootstrapServers");

            await _eventSubscriber.StartAsync(topic, bootstrapServers, groupId, stoppingToken);
        }
    }
}