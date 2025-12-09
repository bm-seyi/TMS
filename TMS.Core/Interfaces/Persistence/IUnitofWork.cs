using Models.Dtos;

namespace TMS.Core.Interfaces.Persistence
{
    public interface IUnitofWork
    {
        Task BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
