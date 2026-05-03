using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using TMS.Application.Queries;
using TMS.Domain.DTOs;


namespace TMS.SignalR.Hubs;

internal sealed class MapHub(ILogger<MapHub> logger, IMediator mediator) : Hub
{
    private readonly ILogger<MapHub> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly static ActivitySource _activitySource = new ActivitySource("TMS.SignalR");

    public override async Task OnConnectedAsync()
    {
        using Activity? activity = _activitySource.StartActivity("LinesDataHub.OnConnectedAsync");

        IEnumerable<LinesReadDTO> linesReadDTOs = await _mediator.Send(new LinesDataQuery(), Context.ConnectionAborted);

        _logger.LogInformation("Client connected to LinesDataHub: {ConnectionId}", Context.ConnectionId);

        await Clients.Caller.SendAsync("MapLinesLoaded", linesReadDTOs, Context.ConnectionAborted);
    }
}
