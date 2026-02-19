using Microsoft.AspNetCore.SignalR.Client;

namespace TMS.Infrastructure.HubClients
{
    internal abstract class HubClientBase : IAsyncDisposable
    {
        protected readonly HubConnection _hubConnection;

        protected HubClientBase(HubConnection hubConnection)
        {
            _hubConnection = hubConnection ?? throw new ArgumentNullException(nameof(hubConnection));
        }

        public Task StartAsync(CancellationToken token) =>  _hubConnection.StartAsync(token);

        public Task StopAsync(CancellationToken token) =>  _hubConnection.StopAsync(token);
        
        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}