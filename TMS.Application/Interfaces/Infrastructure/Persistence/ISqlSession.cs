using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace TMS.Application.Interfaces.Infrastructure.Persistence
{
    public interface ISqlSession
    {
        SqlConnection Connection { get; }
        DbTransaction? Transaction { get;}
        Task OpenAsync(CancellationToken cancellationToken = default);
        Task BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
