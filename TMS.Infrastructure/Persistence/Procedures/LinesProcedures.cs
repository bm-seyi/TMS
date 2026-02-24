using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Persistence;
using TMS.Application.Interfaces.Infrastructure.Persistence.Procedures;
using TMS.Domain.DTOs;

namespace TMS.Infrastructure.Persistence.Procedures
{
    internal sealed class LinesProcedures : ILinesProcedures
    {
        private readonly ILogger<LinesProcedures> _logger;
        private readonly ISqlSession _sqlSession;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Infrastructure.Persistence.Procedures.LinesProcedures");

        public LinesProcedures(ILogger<LinesProcedures> logger, ISqlSession sqlSession)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sqlSession = sqlSession ?? throw new ArgumentNullException(nameof(sqlSession));
        }

        public async Task<IEnumerable<LinesReadDTO>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("LinesProcedures.RetrieveLinesDataAsync");

            _logger.LogDebug("Retrieving Lines Data.");

            CommandDefinition commandDefinition = new CommandDefinition("usp_GetLinesData", null, _sqlSession.Transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            IEnumerable<LinesReadDTO> result = await _sqlSession.Connection.QueryAsync<LinesReadDTO>(commandDefinition);

            return result;
        }
    }
}