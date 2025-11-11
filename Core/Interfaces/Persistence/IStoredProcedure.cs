using Models.Dtos;

namespace Core.Interfaces.Persistence
{
    public interface IStoredProcedureExecutor
    {
        Task<DatabaseHealthCheckDto> Usp_DatabaseHealthCheckAsync(CancellationToken cancellationToken);
    }
}