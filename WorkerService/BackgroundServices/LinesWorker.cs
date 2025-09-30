
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using SignalR.Hubs;

namespace WorkerService.BackgroundServices
{
    public sealed class LinesWorker : BackgroundService
    {
        private readonly ILogger<LinesWorker> _logger;
        private readonly IHubContext<LinesHub> _hubContext;

        public LinesWorker(ILogger<LinesWorker> logger, IHubContext<LinesHub> hubContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "lines-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe("sqlserver.dob.lines");

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string> consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received: {Message}", consumeResult.Message.Value);

                await _hubContext.Clients.All.SendAsync("ReceiveLineUpdate", consumeResult.Message.Value, stoppingToken);
            }
        }
    }
}