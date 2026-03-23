using Blend.Api.Home.Models;

namespace Blend.Api.Home.Services;

/// <summary>
/// Aggregates all home page sections in a single call (per ADR 0006 aggregation pattern).
/// </summary>
public interface IHomeService
{
    /// <summary>
    /// Fetches all home page sections in parallel and returns a combined response.
    /// Guest users (<paramref name="userId"/> = <c>null</c>) receive featured and community
    /// content but an empty recently-viewed section.
    /// </summary>
    Task<HomeResponse> GetHomeAsync(string? userId, CancellationToken ct = default);
}
