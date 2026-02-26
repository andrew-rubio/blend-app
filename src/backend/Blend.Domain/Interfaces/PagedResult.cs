namespace Blend.Domain.Interfaces;

/// <summary>
/// Page of results with a cursor token for keyset pagination (ADR 0006).
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public string? ContinuationToken { get; init; }

    public bool HasMore => ContinuationToken is not null;

    public int Count => Items.Count;
}

/// <summary>
/// Options for paginated queries.
/// </summary>
public class PaginationOptions
{
    public int PageSize { get; init; } = 20;

    public string? ContinuationToken { get; init; }
}
