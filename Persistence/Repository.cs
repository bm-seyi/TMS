using Microsoft.Data.SqlClient;
using System.Text;
using System.Data;
using System.Data.Common;
using Dapper;
using Core.Interfaces.Persistence;
using System.Diagnostics;


namespace Persistence
{
    /// <summary>
    /// This is generic class used to implement the repository pattern
    /// </summary>
    public abstract class Repository : IRepository
    {
        protected readonly SqlConnection _sqlConnection;
        protected readonly DbTransaction? _dbTransaction;
        protected readonly ActivitySource _activitySource;
        protected Repository(SqlConnection sqlConnection, DbTransaction? dbTransaction, string activitySourceName)
        {
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            _dbTransaction = dbTransaction;
            _activitySource = new ActivitySource(activitySourceName);
        }
       
        public required string TableName { get; set; }
        public required string PrimaryKey { get; set; }
 
        /// <summary>
        /// Asynchronously adds a new entity to the database using a generated insert query.
        /// </summary>
        /// <param name="entity">The entity object to be inserted into the database.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("AddAsync");

            CheckTransaction();
 
            string query = GenerateInsertQuery<T>();
 
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
        public virtual async Task<T?> GetAsync<T>(object id, CancellationToken cancellationToken = default) where T : class
        {
            using Activity? activity = _activitySource.StartActivity("GetAsync");
             
            string query = $"SELECT * FROM [dbo].[{TableName}] WHERE [{PrimaryKey}] = @{PrimaryKey}";
 
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(PrimaryKey, id.ToString());
 
            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken);
            IEnumerable<T> data = await _sqlConnection.QueryAsync<T>(commandDefinition);
 
            if (!data.Any())
                return null;
   
            return data.First();
        }
 
        /// <summary>
        /// Asynchronously retrieves all records of type <typeparamref name="T"/> from the database table specified in the repository model.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be returned, which must match the structure of the database table.</typeparam>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing an <see cref="IEnumerable{T}"/> of the retrieved records.</returns>
        public virtual async Task<IEnumerable<T>> GetAsync<T>(CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("GetAsync");

            string query = $"SELECT * FROM [dbo].[{TableName}]";
 
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
        public virtual async Task<T?> GetAsync<T>(object id, string columnName, CancellationToken cancellationToken = default) where T : class
        {
            using Activity? activity = _activitySource.StartActivity("GetAsync");

            string query = $"SELECT * FROM [dbo].[{TableName}] WHERE [{columnName}] = @{columnName}";
 
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(columnName, id.ToString());
 
            CommandDefinition commandDefinition = new CommandDefinition(query, parameters: dynamicParameters, cancellationToken: cancellationToken);
            IEnumerable<T> data = await _sqlConnection.QueryAsync<T>(commandDefinition);
 
            if (!data.Any())
                return null;
 
            return data.First();
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
        public virtual async Task<int> UpdateAsync<T>(object Id, T entity, CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("UpdateAsync");

            CheckTransaction();
 
            IEnumerable<string> cols = GenerateListOfProperties<T>().Select(p => $"[{p}] = @{p}");
            string query = $"UPDATE [dbo].[{TableName}] SET {string.Join(",", cols)} WHERE {PrimaryKey} = @{PrimaryKey}";
 
            DynamicParameters dynamicParameters = new DynamicParameters(entity);
            dynamicParameters.Add(PrimaryKey, Id.ToString());
 
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
        public virtual async Task<int> DeleteAsync(object Id, CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("DeleteAsync");

            CheckTransaction();
 
            string query = $"DELETE FROM [dbo].[{TableName}] WHERE [{PrimaryKey}] = @{PrimaryKey}";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add(PrimaryKey, Id.ToString());
 
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
        private string GenerateInsertQuery<T>()
        {
            using Activity? activity = _activitySource.StartActivity("GenerateInsertQuery");

            List<string> properties = GenerateListOfProperties<T>();
 
            IEnumerable<string> valuesCols = properties.Select(p => $"@{p}");
            StringBuilder insertQuery = new StringBuilder($"INSERT INTO [dbo].[{TableName}] ({string.Join(", ", properties)}) VALUES ({string.Join(", ", valuesCols)})");
 
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
            using Activity? activity = _activitySource.StartActivity("CheckTransaction");
            
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
 