namespace Core.Interfaces.Persistence
{
    public interface IRepository
    {
        Task AddAsync<T>(T entity, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(object id, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> GetAsync<T>(CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(object id, string columnName, CancellationToken cancellationToken = default) where T : class;
        Task<int> UpdateAsync<T>(object Id, T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(object Id, CancellationToken cancellationToken = default);
        
    }
}
