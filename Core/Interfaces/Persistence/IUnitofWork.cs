using TMS.Models.Dtos;

namespace TMS.Core.Interfaces.Persistence
{
    public interface IUnitofWork : IAsyncDisposable
    {
        Task OpenAsync(CancellationToken cancellationToken = default);
        Task BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task CloseAsync();
        Task<DatabaseHealthCheckDto> HealthCheckAsync(CancellationToken cancellationToken = default);
    }
}
