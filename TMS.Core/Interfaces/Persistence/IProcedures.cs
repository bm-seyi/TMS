using Models.Dtos;

namespace TMS.Core.Interfaces.Persistence
{
    public interface IProcedures
    {
        Task<DatabaseHealthCheckDto> Usp_DatabaseHealthCheckAsync(CancellationToken cancellationToken);
    }
}