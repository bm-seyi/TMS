using Microsoft.AspNetCore.SignalR.Client;

namespace TMS.Core.Interfaces.Factories
{
    public interface IHubConnectionFactory
    {
        HubConnection CreateConnection(string relativeHubUrl);
    }
}