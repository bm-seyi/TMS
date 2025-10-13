using Microsoft.AspNetCore.SignalR.Client;

namespace Core.Interfaces.Factories
{
    public interface IHubConnectionFactory
    {
        HubConnection CreateConnection(string relativeHubUrl);
    }
}