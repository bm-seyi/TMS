using TMS.Domain.DTOs;

namespace TMS.Application.Interfaces.Infrastructure.Persistence.Procedures
{
    public interface ILinesProcedures
    {
        Task<IEnumerable<LinesReadDTO>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default);
    }
}