using TMS.Models.DTOs;

namespace TMS.Core.Interfaces.Persistence
{
    public interface IHealthCheckProcedures
    {
        Task<DatabaseHealthCheckDTO> DatabaseHealthCheckAsync(CancellationToken cancellationToken);
    }
}