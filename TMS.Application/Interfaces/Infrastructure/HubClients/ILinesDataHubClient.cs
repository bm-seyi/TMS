namespace TMS.Application.Interfaces.Infrastructure.HubClients
{
    public interface ILinesDataHubClient
    {
        Task NotifyLinesUpdatedAsync(object data, CancellationToken cancellationToken);
    }
}