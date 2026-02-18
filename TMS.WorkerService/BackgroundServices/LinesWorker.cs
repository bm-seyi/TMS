using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using Confluent.Kafka;
using TMS.Application.Interfaces.Services;
using TMS.Application.Interfaces.Factories;
using TMS.Application.Extensions;


namespace TMS.WorkerService.BackgroundServices
{
    public sealed class LinesWorker : BackgroundService
    {
        private readonly ILogger<LinesWorker> _logger;
        private readonly HubConnection _hubConnection;
        private readonly IKafkaService _kafkaService;
        private readonly IConfiguration _configuration;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.WorkerService.BackgroundServices.LinesWorker");

        public LinesWorker(ILogger<LinesWorker> logger, IHubConnectionFactory hubConnectionFactory, IKafkaService kafkaService, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = hubConnectionFactory ?? throw new ArgumentNullException(nameof(hubConnectionFactory));
            _hubConnection = hubConnectionFactory.CreateConnection("linesDataHub");
            _kafkaService = kafkaService ?? throw new ArgumentNullException(nameof(kafkaService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = _activitySource.StartActivity("LinesWorker.ExecuteAsync");

            await _hubConnection.StartAsync(stoppingToken);

            string bootstrapServers = _configuration.GetRequiredValue<string>("Kafka:BootstrapServers");
            await _kafkaService.CreateTopicAsync("sqlserver.TMS.dbo.Lines", bootstrapServers);

            using IConsumer<Ignore, string> consumer = _kafkaService.CreateConsumer(bootstrapServers, "lines-consumer-group");
            consumer.Subscribe("sqlserver.TMS.dbo.Lines");
            
            _logger.LogInformation("LinesWorker subscribed to topic 'sqlserver.TMS.dbo.Lines'");

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received: {Message}", consumeResult.Message.Value);

                await _hubConnection.SendAsync("ReceiveLineUpdate", consumeResult.Message.Value, stoppingToken);
                _logger.LogInformation("ReceiveLineUpdate sent to clients");
            }
        }
    }
}