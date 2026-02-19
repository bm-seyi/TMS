using System.Diagnostics;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Messaging;


namespace TMS.Infrastructure.Messaging
{
    internal sealed class KafkaService : IKafkaService
    {
        private readonly ILogger<KafkaService> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Infrastructure.Messaging.KafkaService");

        public KafkaService(ILogger<KafkaService> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CreateTopicAsync(string topicName, string bootstrapServers)
        {
            using Activity? activity = _activitySource.StartActivity("KafkaLinesEventSubscriber.CreateTopicAsync");

            _logger.LogInformation("Creating Kafka topic: {TopicName}", topicName);

            AdminClientConfig adminConfig = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using IAdminClient adminClient = new AdminClientBuilder(adminConfig).Build();
            try
            {
                Metadata metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                bool topicExists = metadata.Topics.Any(t => t.Topic == topicName && t.Error.Code == ErrorCode.NoError);

                if (!topicExists)
                {
                    _logger.LogInformation("Topic {Topic} not found, creating it...", topicName);

                    await adminClient.CreateTopicsAsync(new List<TopicSpecification>
                    {
                        new TopicSpecification
                        {
                            Name = topicName,
                            NumPartitions = 1,
                            ReplicationFactor = 1
                        }
                    });

                    _logger.LogInformation("Topic {Topic} created successfully", topicName);
                }
                else
                {
                    _logger.LogInformation("Topic {Topic} already exists", topicName);
                }
            }
            catch (CreateTopicsException e)
            {
                if (e.Results.Any(r => r.Error.Code != ErrorCode.TopicAlreadyExists))
                {
                    throw;
                }
            }
        }
    }
}