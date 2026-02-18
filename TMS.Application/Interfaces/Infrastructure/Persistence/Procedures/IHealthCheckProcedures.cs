using TMS.Domain.DTOs;

namespace TMS.Application.Interfaces.Infrastructure.Persistence.Procedures
{
    public interface IHealthCheckProcedures
    {
        Task<DatabaseHealthCheckDTO> DatabaseHealthCheckAsync(CancellationToken cancellationToken = default);
    }
}