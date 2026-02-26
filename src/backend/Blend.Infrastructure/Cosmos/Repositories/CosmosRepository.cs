using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Blend.Infrastructure.Cosmos.Repositories;

/// <summary>
/// Generic Cosmos DB repository implementation.
/// </summary>
/// <typeparam name="T">Domain entity type.</typeparam>
public class CosmosRepository<T> : IRepository<T> where T : CosmosEntity
{
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;

    public CosmosRepository(Container container, ILogger<CosmosRepository<T>> logger)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var response = await _container.CreateItemAsync(
            entity,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Created {Type} with id {Id}", typeof(T).Name, entity.Id);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<T> UpdateAsync(
        T entity,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var response = await _container.ReplaceItemAsync(
            entity,
            entity.Id,
            new PartitionKey(partitionKey),
            cancellationToken: cancellationToken);

        _logger.LogDebug("Updated {Type} with id {Id}", typeof(T).Name, entity.Id);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<T> PatchAsync(
        string id,
        string partitionKey,
        IReadOnlyList<Domain.Interfaces.PatchOperation> operations,
        CancellationToken cancellationToken = default)
    {
        var cosmosPatchOps = operations
            .Select(MapToCosmosPatch)
            .ToList();

        // Always update the UpdatedAt field
        cosmosPatchOps.Add(Microsoft.Azure.Cosmos.PatchOperation.Set("/updatedAt", DateTimeOffset.UtcNow));

        var response = await _container.PatchItemAsync<T>(
            id,
            new PartitionKey(partitionKey),
            cosmosPatchOps,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Patched {Type} with id {Id}", typeof(T).Name, id);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogDebug("Deleted {Type} with id {Id}", typeof(T).Name, id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<PagedResult<T>> QueryAsync(
        string query,
        PaginationOptions? pagination = null,
        IDictionary<string, object>? parameters = null,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var queryDef = BuildQueryDefinition(query, parameters);

        var options = new QueryRequestOptions
        {
            MaxItemCount = pagination?.PageSize ?? 20
        };

        if (partitionKey is not null)
        {
            options.PartitionKey = new PartitionKey(partitionKey);
        }

        using var feed = _container.GetItemQueryIterator<T>(
            queryDef,
            continuationToken: pagination?.ContinuationToken,
            requestOptions: options);

        var items = new List<T>();
        string? continuationToken = null;

        if (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            items.AddRange(page);
            continuationToken = page.ContinuationToken;
        }

        return new PagedResult<T>
        {
            Items = items,
            ContinuationToken = continuationToken
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> ExecuteBatchAsync(
        string partitionKey,
        IReadOnlyList<BatchOperation<T>> operations,
        CancellationToken cancellationToken = default)
    {
        var batch = _container.CreateTransactionalBatch(new PartitionKey(partitionKey));

        foreach (var op in operations)
        {
            switch (op.OperationType)
            {
                case BatchOperationType.Create:
                    batch.CreateItem(op.Entity);
                    break;
                case BatchOperationType.Upsert:
                    batch.UpsertItem(op.Entity);
                    break;
                case BatchOperationType.Replace:
                    batch.ReplaceItem(op.Entity.Id, op.Entity);
                    break;
                case BatchOperationType.Delete:
                    batch.DeleteItem(op.Entity.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op.OperationType), op.OperationType, null);
            }
        }

        using var batchResponse = await batch.ExecuteAsync(cancellationToken);

        if (!batchResponse.IsSuccessStatusCode)
        {
            throw new CosmosException(
                $"Transactional batch failed with status {batchResponse.StatusCode}",
                batchResponse.StatusCode,
                0,
                batchResponse.ActivityId,
                batchResponse.RequestCharge);
        }

        var results = new List<T>();
        for (int i = 0; i < batchResponse.Count; i++)
        {
            var itemResponse = batchResponse.GetOperationResultAtIndex<T>(i);
            if (itemResponse.Resource is not null)
            {
                results.Add(itemResponse.Resource);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, partitionKey, cancellationToken);
        return item is not null;
    }

    private static QueryDefinition BuildQueryDefinition(
        string query,
        IDictionary<string, object>? parameters)
    {
        var def = new QueryDefinition(query);
        if (parameters is not null)
        {
            foreach (var (name, value) in parameters)
            {
                def = def.WithParameter(name, value);
            }
        }
        return def;
    }

    private static Microsoft.Azure.Cosmos.PatchOperation MapToCosmosPatch(
        Domain.Interfaces.PatchOperation op)
    {
        return op.OperationType switch
        {
            Domain.Interfaces.PatchOperationType.Add => Microsoft.Azure.Cosmos.PatchOperation.Add(op.Path, op.Value),
            Domain.Interfaces.PatchOperationType.Remove => Microsoft.Azure.Cosmos.PatchOperation.Remove(op.Path),
            Domain.Interfaces.PatchOperationType.Replace => Microsoft.Azure.Cosmos.PatchOperation.Replace(op.Path, op.Value),
            Domain.Interfaces.PatchOperationType.Set => Microsoft.Azure.Cosmos.PatchOperation.Set(op.Path, op.Value),
            Domain.Interfaces.PatchOperationType.Increment => op.Value is not null
                ? Microsoft.Azure.Cosmos.PatchOperation.Increment(op.Path, Convert.ToDouble(op.Value))
                : throw new ArgumentException("Increment requires a numeric value."),
            _ => throw new ArgumentOutOfRangeException(nameof(op.OperationType), op.OperationType, null)
        };
    }
}
