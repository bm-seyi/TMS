using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Application.Queries;
using TMS.Domain.DTOs;


namespace TMS.Application.Handlers
{
    public sealed class LinesDataHandler(ILogger<LinesDataHandler> logger, ILinesProcedures linesProcedures) : IRequestHandler<LinesDataQuery, IEnumerable<LinesReadDTO>>
    {
        private readonly ILogger<LinesDataHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ILinesProcedures _linesProcedures = linesProcedures ?? throw new ArgumentNullException(nameof(linesProcedures));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Application");

        public async Task<IEnumerable<LinesReadDTO>> Handle(LinesDataQuery request, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("LinesDataHandler.Handle");

            _logger.LogDebug("Handling LinesDataQuery.");

            IEnumerable<LinesReadDTO> linesReadDTOs =  await _linesProcedures.RetrieveLinesDataAsync(cancellationToken);

            _logger.LogDebug("LinesDataQuery handled successfully.");
            return linesReadDTOs;
        }
    }
}