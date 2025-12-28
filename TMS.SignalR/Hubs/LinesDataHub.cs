using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using TMS.Core.Queries;
using TMS.Models.DTOs;


namespace TMS.SignalR.Hubs
{
    public sealed class LinesDataHub : Hub
    {
        private readonly ILogger<LinesDataHub> _logger;
        private readonly IMediator _mediator;
        private readonly static ActivitySource _activitySource = new ActivitySource("TMS.SignalR.Hubs.LinesDataHub");

        public LinesDataHub(ILogger<LinesDataHub> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public override async Task OnConnectedAsync()
        {
            using Activity? activity = _activitySource.StartActivity("LinesDataHub.OnConnectedAsync");

            IEnumerable<LinesReadDTO> linesReadDTOs = await _mediator.Send(new LinesDataQuery(), Context.ConnectionAborted);

            _logger.LogInformation("Client connected to LinesDataHub: {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("ReceiveLines", linesReadDTOs, Context.ConnectionAborted);
        }
    }
}