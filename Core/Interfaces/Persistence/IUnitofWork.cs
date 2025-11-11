using Models.Dtos;

namespace Core.Interfaces.Persistence
{
    public interface IUnitofWork : IAsyncDisposable
    {
        IRepository Lines  { get; }
        Task OpenAsync(CancellationToken cancellationToken = default);
        Task BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
