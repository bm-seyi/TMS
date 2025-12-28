using TMS.Models.DTOs;

namespace TMS.Core.Interfaces.Persistence.Procedures
{
    public interface ILinesProcedures
    {
        Task<IEnumerable<LinesReadDTO>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default);
    }
}