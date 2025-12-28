using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.Persistence.Procedures;
using TMS.Core.Queries;
using TMS.Models.DTOs;


namespace TMS.Core.Handlers
{
    public sealed class LinesDataHandler : IRequestHandler<LinesDataQuery, IEnumerable<LinesReadDTO>>
    {
        private readonly ILogger<LinesDataHandler> _logger;
        private readonly ILinesProcedures _linesProcedures;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Handlers.LinesDataHandler");

        public LinesDataHandler(ILogger<LinesDataHandler> logger, ILinesProcedures linesProcedures)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _linesProcedures = linesProcedures ?? throw new ArgumentNullException(nameof(linesProcedures));
        }

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