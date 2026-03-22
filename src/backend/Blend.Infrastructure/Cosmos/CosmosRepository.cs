using System.Net;
using System.Text;
using System.Text.Json;
using Blend.Domain.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Blend.Infrastructure.Cosmos;

/// <summary>
/// Generic Cosmos DB repository that implements <see cref="IRepository{T}"/> for a single container.
/// </summary>
/// <typeparam name="T">The entity type stored in the container.</typeparam>
public sealed class CosmosRepository<T> : IRepository<T> where T : class
{
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;

    public CosmosRepository(Container container, ILogger<CosmosRepository<T>> logger)
    {
        _container = container;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition(query);
        var options = partitionKey is not null
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : null;

        var results = new List<T>();
        using var feed = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: options);

        while (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            results.AddRange(page);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(entity, cancellationToken: cancellationToken);
        return response.Resource;
    }

    /// <inheritdoc/>
    public async Task<T> UpdateAsync(T entity, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        var response = await _container.ReplaceItemAsync(entity, id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
        return response.Resource;
    }

    /// <inheritdoc/>
    public async Task<T> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default)
    {
        var operations = patches
            .Select(kvp => PatchOperation.Set(kvp.Key, kvp.Value))
            .ToList();

        var response = await _container.PatchItemAsync<T>(id, new PartitionKey(partitionKey), operations, cancellationToken: cancellationToken);
        return response.Resource;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<T>> GetPagedAsync(
        string query,
        FeedPaginationOptions options,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var rawToken = options.ContinuationToken is not null
            ? DecodeContinuationToken(options.ContinuationToken)
            : null;

        var queryOptions = new QueryRequestOptions { MaxItemCount = options.PageSize };
        if (partitionKey is not null)
        {
            queryOptions.PartitionKey = new PartitionKey(partitionKey);
        }

        var results = new List<T>();
        string? nextToken = null;

        using var feed = _container.GetItemQueryIterator<T>(
            new QueryDefinition(query),
            continuationToken: rawToken,
            requestOptions: queryOptions);

        if (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            results.AddRange(page);
            nextToken = page.ContinuationToken is not null
                ? EncodeContinuationToken(page.ContinuationToken)
                : null;
        }

        return new PagedResult<T>
        {
            Items = results,
            ContinuationToken = nextToken,
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<T>> GetOffsetPagedAsync(
        string baseQuery,
        OffsetPaginationOptions options,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var skip = options.Page * options.PageSize;
        var pagedQuery = $"{baseQuery} OFFSET {skip} LIMIT {options.PageSize}";

        var items = await GetByQueryAsync(pagedQuery, partitionKey, cancellationToken);

        // Issue a COUNT query to get total
        var countQuery = BuildCountQuery(baseQuery);
        int? totalCount = null;
        try
        {
            var countResults = await GetByQueryAsync(countQuery, partitionKey, cancellationToken);
            if (countResults.Count == 1)
            {
                // Count result is a dynamic-ish object; deserialise via JSON
                var json = JsonSerializer.Serialize(countResults[0]);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("count", out var countEl)
                    || doc.RootElement.TryGetProperty("$1", out countEl))
                {
                    totalCount = countEl.GetInt32();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve total count for query: {Query}", baseQuery);
        }

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task ExecuteTransactionalBatchAsync(
        string partitionKey,
        IEnumerable<(TransactionalBatchOperation Operation, T Entity)> operations,
        CancellationToken cancellationToken = default)
    {
        var batch = _container.CreateTransactionalBatch(new PartitionKey(partitionKey));

        foreach (var (op, entity) in operations)
        {
            switch (op)
            {
                case TransactionalBatchOperation.Create:
                    batch.CreateItem(entity);
                    break;
                case TransactionalBatchOperation.Replace:
                    var replaceId = GetId(entity);
                    batch.ReplaceItem(replaceId, entity);
                    break;
                case TransactionalBatchOperation.Upsert:
                    batch.UpsertItem(entity);
                    break;
                case TransactionalBatchOperation.Delete:
                    var deleteId = GetId(entity);
                    batch.DeleteItem(deleteId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, "Unknown transactional batch operation.");
            }
        }

        using var response = await batch.ExecuteAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CosmosException(
                $"Transactional batch failed with status {response.StatusCode}",
                response.StatusCode,
                (int)response.StatusCode,
                response.ActivityId,
                response.RequestCharge);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string EncodeContinuationToken(string rawToken) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

    private static string DecodeContinuationToken(string encodedToken) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(encodedToken));

    private static string BuildCountQuery(string baseQuery)
    {
        // Wrap the original query in a COUNT subquery
        var trimmed = baseQuery.TrimEnd(';');
        return $"SELECT VALUE COUNT(1) FROM ({trimmed}) AS c";
    }

    private static string GetId(T entity)
    {
        // Rely on the "id" property via reflection (all Blend entities expose it)
        var prop = typeof(T).GetProperty("Id")
                   ?? throw new InvalidOperationException($"Entity type {typeof(T).Name} does not expose an 'Id' property.");
        return prop.GetValue(entity)?.ToString()
               ?? throw new InvalidOperationException($"Entity of type {typeof(T).Name} has a null Id.");
    }
}
