using Blend.Domain.Entities;

namespace Blend.Domain.Interfaces;

/// <summary>
/// Generic repository interface for Cosmos DB containers.
/// Provides CRUD, pagination, partial updates, and transactional batch operations.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="CosmosEntity"/>.</typeparam>
public interface IRepository<T> where T : CosmosEntity
{
    /// <summary>Gets a document by its ID and partition key value.</summary>
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>Creates a new document. Returns the created document.</summary>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing document entirely. Returns the updated document.</summary>
    Task<T> UpdateAsync(T entity, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update (patch) to a document using Cosmos DB patch operations.
    /// </summary>
    Task<T> PatchAsync(
        string id,
        string partitionKey,
        IReadOnlyList<PatchOperation> operations,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a document by its ID and partition key. Returns false if not found.</summary>
    Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an arbitrary SQL query and returns a paged result.
    /// </summary>
    Task<PagedResult<T>> QueryAsync(
        string query,
        PaginationOptions? pagination = null,
        IDictionary<string, object>? parameters = null,
        string? partitionKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a transactional batch within a single logical partition.
    /// </summary>
    Task<IReadOnlyList<T>> ExecuteBatchAsync(
        string partitionKey,
        IReadOnlyList<BatchOperation<T>> operations,
        CancellationToken cancellationToken = default);

    /// <summary>Returns true if a document exists.</summary>
    Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
}

/// <summary>Operation in a transactional batch.</summary>
public record BatchOperation<T>(BatchOperationType OperationType, T Entity) where T : CosmosEntity;

public enum BatchOperationType
{
    Create,
    Upsert,
    Replace,
    Delete
}

/// <summary>
/// Represents a single patch operation for partial document updates.
/// </summary>
public record PatchOperation(PatchOperationType OperationType, string Path, object? Value = null);

public enum PatchOperationType
{
    Add,
    Remove,
    Replace,
    Set,
    Increment
}
