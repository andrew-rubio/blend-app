using Blend.Domain.Identity;
using Blend.Domain.Repositories;

namespace Blend.Api.Auth;

/// <summary>
/// No-op repository used when Cosmos DB is not configured (e.g. during tests or local dev
/// without a database). Auth operations will throw if actually invoked without a real store.
/// </summary>
internal sealed class NullBlendUserRepository : IRepository<BlendUser>
{
    private static NotSupportedException Unsupported() =>
        new("Cosmos DB is required for authentication. Configure CosmosDb in appsettings.");

    public Task<BlendUser?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task<IReadOnlyList<BlendUser>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task<BlendUser> CreateAsync(BlendUser entity, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task<BlendUser> UpdateAsync(BlendUser entity, string id, string partitionKey, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task PatchAsync(string id, string partitionKey, IEnumerable<(string Path, object? Value)> patches, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task<PagedResult<BlendUser>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task<PagedResult<BlendUser>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw Unsupported();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, BlendUser Entity)> operations, CancellationToken cancellationToken = default)
        => throw Unsupported();
}
