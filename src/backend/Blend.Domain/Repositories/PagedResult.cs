namespace Blend.Domain.Repositories;

/// <summary>
/// Represents a paginated result set with a continuation token for cursor-based pagination.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Gets the items returned for this page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the Base64-encoded continuation token for the next page, or <c>null</c> if this is the last page.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>Gets the total number of items, if known (used for offset-based pagination).</summary>
    public int? TotalCount { get; init; }

    /// <summary>Gets whether there are more pages after this one.</summary>
    public bool HasNextPage => ContinuationToken is not null;
}

/// <summary>
/// Parameters for cursor-based (feed) pagination.
/// </summary>
public sealed class FeedPaginationOptions
{
    /// <summary>Maximum number of items to return per page. Defaults to 20.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// The Base64-encoded continuation token from a previous <see cref="PagedResult{T}"/>, or <c>null</c> for the first page.
    /// </summary>
    public string? ContinuationToken { get; init; }
}

/// <summary>
/// Parameters for offset-based (search) pagination.
/// </summary>
public sealed class OffsetPaginationOptions
{
    /// <summary>Zero-based page number.</summary>
    public int Page { get; init; } = 0;

    /// <summary>Items per page. Defaults to 20.</summary>
    public int PageSize { get; init; } = 20;
}
