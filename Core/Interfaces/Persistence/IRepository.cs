namespace TMS.Core.Interfaces.Persistence
{
    public interface IRepository
    {
        Task AddAsync<T>(T entity, string tableName, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAsync<T>(string tableName, string primaryKey, object id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAsync<T>(string tableName, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAsync<T>(string tableName, object id, string columnName, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync<T>(string tableName, string primaryKey, object Id, T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(string tableName, string primaryKey, object Id, CancellationToken cancellationToken = default);
        
    }
}
