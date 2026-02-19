
namespace TMS.Application.Interfaces.Infrastructure.Messaging
{
    public interface IKafkaService
    {
        Task CreateTopicAsync(string topicName, string bootstrapServers);
    }
}