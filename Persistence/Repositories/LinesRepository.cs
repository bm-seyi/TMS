using Microsoft.Data.SqlClient;
using System.Data.Common;
using FlightSearchEngine.Persistence;

namespace Persistence.Repositories
{
    public sealed class LinesRepository : Repository
    {
        public LinesRepository(SqlConnection sqlConnection, DbTransaction? sqlTransaction) : base(sqlConnection, sqlTransaction) {}        
    }
}