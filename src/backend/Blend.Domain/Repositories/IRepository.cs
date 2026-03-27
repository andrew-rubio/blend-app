namespace Blend.Domain.Repositories;

/// <summary>
/// Generic repository interface providing CRUD and query operations for a domain entity.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Gets an entity by its unique identifier and partition key value.</summary>
    /// <param name="id">The document id.</param>
    /// <param name="partitionKey">The partition key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities matching the given SQL query string.</summary>
    /// <param name="query">The Cosmos DB SQL query.</param>
    /// <param name="partitionKey">Optional partition key to scope the query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<T>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities matching the given parameterized SQL query.</summary>
    /// <param name="query">The Cosmos DB SQL query with @parameter placeholders.</param>
    /// <param name="parameters">Dictionary of parameter names (with @) to values.</param>
    /// <param name="partitionKey">Optional partition key to scope the query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<T>> GetByQueryAsync(string query, IReadOnlyDictionary<string, object> parameters, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        // Default: substitute parameters into the query string so implementations
        // that only understand the non-parameterized overload can parse actual values.
        var resolved = query;
        foreach (var kvp in parameters)
        {
            resolved = resolved.Replace(kvp.Key, kvp.Value switch
            {
                string s => $"'{s}'",
                bool b => b ? "true" : "false",
                _ => kvp.Value?.ToString() ?? "null",
            });
        }
        return GetByQueryAsync(resolved, partitionKey, cancellationToken);
    }

    /// <summary>Creates a new entity document.</summary>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing entity document (full replace).</summary>
    Task<T> UpdateAsync(T entity, string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>Applies a partial patch to an existing entity.</summary>
    /// <param name="id">Document id.</param>
    /// <param name="partitionKey">Partition key value.</param>
    /// <param name="patches">Dictionary of JSON Pointer paths to new values.</param>
    Task<T> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity by id and partition key.</summary>
    Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>Returns a page of results using cursor-based (continuation token) pagination.</summary>
    Task<PagedResult<T>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>Returns a page of results using offset-based pagination (for search results).</summary>
    Task<PagedResult<T>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a set of operations in a transactional batch scoped to a single partition key.
    /// </summary>
    Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, T Entity)> operations, CancellationToken cancellationToken = default);
}

/// <summary>Represents a transactional batch operation type.</summary>
public enum TransactionalBatchOperation
{
    Create,
    Replace,
    Delete,
    Upsert,
}
