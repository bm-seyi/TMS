using Microsoft.AspNetCore.SignalR;
using TMS.Core.Interfaces.Persistence;
using TMS.Models.DTOs;

namespace SignalR.Hubs
{
    public sealed class LinesHub : Hub
    {
        private readonly ILogger<LinesHub> _logger;

        public LinesHub(ILogger<LinesHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to LinesHub: {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("ReceiveLines", new {}, Context.ConnectionAborted);
        }
    }
}