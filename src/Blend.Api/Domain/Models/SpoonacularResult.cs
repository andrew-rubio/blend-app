namespace Blend.Api.Domain.Models;

/// <summary>
/// Wraps a Spoonacular service result with metadata about data origin and service health.
/// </summary>
/// <typeparam name="T">The result payload type.</typeparam>
public record SpoonacularResult<T>
{
    public required T Data { get; init; }

    /// <summary>
    /// Indicates where the data was sourced from: "spoonacular", "l1-cache", "l2-cache", or "none".
    /// </summary>
    public required string DataSource { get; init; }

    /// <summary>
    /// True when the data came from a cache tier rather than the live API.
    /// </summary>
    public bool IsFromCache => DataSource is "l1-cache" or "l2-cache";

    /// <summary>
    /// True when the service is rate-limited or degraded and results may be incomplete.
    /// </summary>
    public bool IsLimitedResults { get; init; }
}
