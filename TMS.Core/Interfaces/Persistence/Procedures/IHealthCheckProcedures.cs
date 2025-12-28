using TMS.Models.DTOs;

namespace TMS.Core.Interfaces.Persistence.Procedures
{
    public interface IHealthCheckProcedures
    {
        Task<DatabaseHealthCheckDTO> DatabaseHealthCheckAsync(CancellationToken cancellationToken = default);
    }
}