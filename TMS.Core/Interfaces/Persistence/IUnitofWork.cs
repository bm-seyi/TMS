namespace TMS.Core.Interfaces.Persistence
{
    public interface IUnitofWork
    {
        IProcedures Procedures { get; }
        Task BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
