using Microsoft.AspNetCore.SignalR;
using Confluent.Kafka;
using SignalR.Hubs;
using Core.Interfaces.Factories;
using System.Diagnostics;

namespace WorkerService.BackgroundServices
{
    public sealed class LinesWorker : BackgroundService
    {
        private readonly ILogger<LinesWorker> _logger;
        private readonly IHubContext<LinesHub> _hubContext;
        private readonly IKafkaService _kafkaService;
        private readonly IConfiguration _configuration;
        private static readonly ActivitySource _activitySource = new ActivitySource("WorkerService.BackgroundServices.LinesWorker");

        public LinesWorker(ILogger<LinesWorker> logger, IHubContext<LinesHub> hubContext, IKafkaService kafkaService, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _kafkaService = kafkaService ?? throw new ArgumentNullException(nameof(kafkaService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = _activitySource.StartActivity("ExecuteAsync");
            
            string bootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers") ?? throw new InvalidOperationException("Kafka BootstrapServers configuration is missing.");
            await _kafkaService.CreateTopicAsync("sqlserver.TMS.dbo.Lines", bootstrapServers);

            using IConsumer<Ignore, string> consumer = _kafkaService.CreateConsumer(bootstrapServers, "lines-consumer-group");
            consumer.Subscribe("sqlserver.TMS.dbo.Lines");
            _logger.LogInformation("LinesWorker subscribed to topic 'sqlserver.TMS.dbo.Lines'");

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received: {Message}", consumeResult.Message.Value);

                await _hubContext.Clients.All.SendAsync("ReceiveLineUpdate", consumeResult.Message.Value, stoppingToken);
                _logger.LogInformation("ReceiveLineUpdate sent to clients");
            }
        }
    }
}