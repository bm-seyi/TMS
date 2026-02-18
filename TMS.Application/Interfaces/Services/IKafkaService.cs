using Confluent.Kafka;

namespace TMS.Application.Interfaces.Services
{
    public interface IKafkaService
    {
        Task CreateTopicAsync(string topicName, string bootstrapServers);
        IConsumer<Ignore, string> CreateConsumer(string bootstrapServers, string groupId);
    }
}