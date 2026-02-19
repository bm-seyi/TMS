using System.Diagnostics;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TMS.Application.Events;
using TMS.Application.Interfaces.Infrastructure.Messaging;


namespace TMS.Infrastructure.Messaging
{
    internal sealed class KafkaLinesEventSubscriber : IEventSubscriber
    {
        private readonly ILogger<KafkaLinesEventSubscriber> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Infrastructure.Messaging.KafkaLinesEventSubscriber");

        public KafkaLinesEventSubscriber(ILogger<KafkaLinesEventSubscriber> logger, IKafkaService kafkaService, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _kafkaService = kafkaService ?? throw new ArgumentNullException(nameof(kafkaService));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task StartAsync(string topicName, string bootstrapServers, string GroupId, CancellationToken cancellationToken)
        {
            using Activity? _ = _activitySource.StartActivity("KafkaLinesEventSubscriber.StartAsync");

            await _kafkaService.CreateTopicAsync(topicName, bootstrapServers);

            ConsumerConfig config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using IConsumer<Ignore, string>? consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topicName);

            _logger.LogInformation("Subscribed to topic {Topic}", topicName);

            while (!cancellationToken.IsCancellationRequested)
            {  
                using var scope = _serviceScopeFactory.CreateScope();
                IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                ConsumeResult<Ignore, string> result = consumer.Consume(cancellationToken);

                LinesUpdatedNotification domainEvent = new LinesUpdatedNotification(result.Message.Value);

                await mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}