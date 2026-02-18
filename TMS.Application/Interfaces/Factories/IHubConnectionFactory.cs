using Microsoft.AspNetCore.SignalR.Client;

namespace TMS.Application.Interfaces.Factories
{
    public interface IHubConnectionFactory
    {
        HubConnection CreateConnection(string relativeHubUrl);
    }
}