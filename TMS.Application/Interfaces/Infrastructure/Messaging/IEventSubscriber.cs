namespace TMS.Application.Interfaces.Infrastructure.Messaging
{
    public interface IEventSubscriber
    {
        Task StartAsync(string topicName, string bootstrapServers, string GroupId, CancellationToken cancellationToken);
    }
}