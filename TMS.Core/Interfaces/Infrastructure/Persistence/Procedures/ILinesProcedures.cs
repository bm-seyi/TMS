using TMS.Domain.DTOs;

namespace TMS.Core.Interfaces.Infrastructure.Persistence.Procedures
{
    public interface ILinesProcedures
    {
        Task<IEnumerable<LinesReadDTO>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default);
    }
}