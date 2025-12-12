using Microsoft.AspNetCore.SignalR;
using TMS.Core.Interfaces.Persistence;
using TMS.Models.DTOs;

namespace SignalR.Hubs
{
    public sealed class LinesHub : Hub
    {
        private readonly ILogger<LinesHub> _logger;
        private readonly IUnitofWork _unitOfWork;

        public LinesHub(ILogger<LinesHub> logger, IUnitofWork unitofWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitofWork ?? throw new ArgumentNullException(nameof(unitofWork));
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected to LinesHub: {ConnectionId}", Context.ConnectionId);


            await Clients.Caller.SendAsync("ReceiveLines", new {}, Context.ConnectionAborted);
        }
    }
}