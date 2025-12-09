using TMS.Models.DTOs;

namespace TMS.Core.Interfaces.Persistence
{
    public interface IProcedures
    {
        Task<DatabaseHealthCheckDTO> Usp_DatabaseHealthCheckAsync(CancellationToken cancellationToken);
    }
}