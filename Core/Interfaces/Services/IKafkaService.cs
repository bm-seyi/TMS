using Confluent.Kafka;

namespace Core.Interfaces.Factories
{
    public interface IKafkaService
    {
        Task CreateTopicAsync(string topicName, string bootstrapServers);
        IConsumer<Ignore, string> CreateConsumer(string bootstrapServers, string groupId);
    }
}