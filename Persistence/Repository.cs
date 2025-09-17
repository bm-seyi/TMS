using Microsoft.Data.SqlClient;
using System.Text;
using System.Data;
using System.Data.Common;
using Dapper;
using Core.Interfaces.Persistence;


namespace Persistence
{
    /// <summary>
    /// This is generic class used to implement the repository pattern
    /// </summary>
    public class Repository : IRepository
    {
        private readonly SqlConnection _sqlConnection;
        private readonly DbTransaction? _dbTransaction;

        public Repository(SqlConnection sqlConnection, DbTransaction? dbTransaction)
        {
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            _dbTransaction = dbTransaction;
        }

        /// <summary>
        /// Asynchronously adds a new entity to the database using a generated insert query.
        /// </summary>
        /// <param name="entity">The entity object to be inserted into the database.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync<T>(T entity, string tableName, CancellationToken cancellationToken = default)
        {
            CheckTransaction();

            string query = GenerateInsertQuery<T>(tableName);
        
            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: entity, cancellationToken: cancellationToken, transaction: _dbTransaction);
            await _sqlConnection.ExecuteAsync(commandDefinition);
        }

        /// <summary>
        /// Asynchronously retrieves a collection of records of type <typeparamref name="T"/> from the database
        /// where the record's ID matches the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique identifier used to filter the records.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of matching records.</returns>
        public async Task<IEnumerable<T>> GetAsync<T>(string tableName, string primaryKey, object id, CancellationToken cancellationToken = default)
        {            
            string query = $"SELECT * FROM [dbo].[{tableName}] WHERE [{primaryKey}] = @{primaryKey}";

            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(primaryKey, id.ToString());

            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken);
            IEnumerable<T> data = await _sqlConnection.QueryAsync<T>(commandDefinition);

            return data;
        }

        /// <summary>
        /// Asynchronously retrieves all records of type <typeparamref name="T"/> from the database table specified in the repository model.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be returned, which must match the structure of the database table.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing an <see cref="IEnumerable{T}"/> of the retrieved records.</returns>
        public async Task<IEnumerable<T>> GetAsync<T>(string tableName, CancellationToken cancellationToken = default)
        {
            string query = $"SELECT * FROM [dbo].[{tableName}]";

            CommandDefinition commandDefinition = new CommandDefinition(query, cancellationToken: cancellationToken);
            IEnumerable<T> data = await _sqlConnection.QueryAsync<T>(commandDefinition);

            return data;
        }


        /// <summary>
        /// Asynchronously retrieves a collection of entities of type <typeparamref name="T"/> from the database table 
        /// specified in the repository model, filtered by a column and its corresponding value.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="id">The value to match in the specified column.</param>
        /// <param name="columnName">The name of the column to filter by.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the filtered collection of entities.</returns>
        public async Task<IEnumerable<T>> GetAsync<T>(string tableName, object id, string columnName, CancellationToken cancellationToken = default)
        {
            string query = $"SELECT * FROM [dbo].[{tableName}] WHERE [{columnName}] = @{columnName}";

            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(columnName, id.ToString());

            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken);
            IEnumerable<T> data = await _sqlConnection.QueryAsync<T>(commandDefinition);

            return data;
        }
        
        /// <summary>
        /// Asynchronously updates a record of type <typeparamref name="T"/> in the database table specified in the repository model.
        /// </summary>
        /// <typeparam name="T">The type of the entity being updated, which must match the structure of the database table.</typeparam>
        /// <param name="Id">The identifier of the record to update, corresponding to the primary key.</param>
        /// <param name="entity">The entity containing updated values for the record.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the number of rows affected.
        /// Throws <see cref="InvalidOperationException"/> if more than one row is affected.
        /// </returns>
        public async Task<int> UpdateAsync<T>(string tableName, string primaryKey, object Id, T entity, CancellationToken cancellationToken = default)
        {
            CheckTransaction();

            IEnumerable<string> cols = GenerateListOfProperties<T>().Select(p => $"[{p}] = @{p}");
            string query = $"UPDATE [dbo].[{tableName}] SET {string.Join(",", cols)} WHERE {primaryKey} = @{primaryKey}";

            DynamicParameters dynamicParameters = new DynamicParameters(entity);
            dynamicParameters.Add(primaryKey, Id.ToString());

            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken, transaction: _dbTransaction);
            int rowsAffected = await _sqlConnection.ExecuteAsync(commandDefinition);

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Delete operation affected {rowsAffected} rows; expected only one row to be deleted.");
            }

            return rowsAffected;
        }


        /// <summary>
        /// Asynchronously deletes a record from the database table specified in the repository model using the provided identifier.
        /// </summary>
        /// <param name="Id">The identifier of the record to delete, corresponding to the primary key.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the number of rows affected.
        /// Throws <see cref="InvalidOperationException"/> if more than one row is affected.
        /// </returns>
        public async Task<int> DeleteAsync(string tableName, string primaryKey, object Id, CancellationToken cancellationToken = default)
        {
            CheckTransaction();

            string query = $"DELETE FROM [dbo].[{tableName}] WHERE [{primaryKey}] = @{primaryKey}";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(primaryKey, Id.ToString());

            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken, transaction: _dbTransaction);
            int rowsAffected = await _sqlConnection.ExecuteAsync(commandDefinition);

            if (rowsAffected > 1)
            {
                throw new InvalidOperationException($"Delete operation affected {rowsAffected} rows; expected only one row to be deleted.");
            }

            return rowsAffected;
        }

        /// <summary>
        /// Generates a SQL INSERT query string based on the properties of the entity type.
        /// </summary>
        /// <returns>
        /// A SQL INSERT statement as a string, with parameter placeholders for each property.
        /// </returns>
        private static string GenerateInsertQuery<T>(string tableName)
        {
            List<string> properties = GenerateListOfProperties<T>();

            IEnumerable<string> valuesCols = properties.Select(p => $"@{p}");
            StringBuilder insertQuery = new StringBuilder($"INSERT INTO [dbo].[{tableName}] ({string.Join(", ", properties)}) VALUES ({string.Join(", ", valuesCols)})");

            return insertQuery.ToString();
        }


        
        /// <summary>
        /// Validates that a database transaction is active before performing operations that require it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current transaction is <c>null</c>, indicating that the operation cannot proceed without an active transaction.
        /// </exception>
        private void CheckTransaction()
        {
            if (_dbTransaction == null)
                throw new InvalidOperationException("The transaction can't not be null for this operation");
        }


        /// <summary>
        /// Asynchronously creates and opens a SQL database connection using the default connection string.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an open <see cref="SqlConnection"/>.
        /// </returns>
        private static List<string> GenerateListOfProperties<T>() => typeof(T).GetProperties().Select(prop => prop.Name).ToList();
    }
}