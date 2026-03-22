namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// Indicates where the data in a <see cref="SpoonacularResult{T}"/> originated.
/// </summary>
public enum DataSource
{
    /// <summary>Data was returned from the live Spoonacular API.</summary>
    Spoonacular,

    /// <summary>Data was served from the L1 or L2 cache.</summary>
    Cache,

    /// <summary>
    /// Data could not be retrieved (Spoonacular unavailable and no cached copy).
    /// The result set will be empty and <see cref="SpoonacularResult{T}.IsAvailable"/> will be false.
    /// </summary>
    Degraded,
}

/// <summary>
/// Wrapper returned by every <see cref="ISpoonacularService"/> method. Carries the payload,
/// provenance information, and degradation flags (PLAT-38, PLAT-41).
/// </summary>
/// <typeparam name="T">The result payload type.</typeparam>
public sealed class SpoonacularResult<T>
{
    /// <summary>The result payload. Will be the type's default when <see cref="IsAvailable"/> is false.</summary>
    public T? Data { get; init; }

    /// <summary>True when results are included in <see cref="Data"/>.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Where the data originated (API, cache, or degraded).</summary>
    public DataSource DataSource { get; init; }

    /// <summary>
    /// True when the service is operating in cache-only mode due to API quota exhaustion.
    /// Clients should display a "limited results" indicator when this is true.
    /// </summary>
    public bool IsLimited { get; init; }

    // ── Factory helpers ────────────────────────────────────────────────────────

    /// <summary>Returns a successful result with data from the live API.</summary>
    public static SpoonacularResult<T> FromApi(T data) =>
        new() { Data = data, IsAvailable = true, DataSource = DataSource.Spoonacular };

    /// <summary>Returns a successful result with data from the cache.</summary>
    public static SpoonacularResult<T> FromCache(T data) =>
        new() { Data = data, IsAvailable = true, DataSource = DataSource.Cache };

    /// <summary>Returns a successful result from the cache when the API is rate-limited.</summary>
    public static SpoonacularResult<T> FromCacheLimited(T data) =>
        new() { Data = data, IsAvailable = true, DataSource = DataSource.Cache, IsLimited = true };

    /// <summary>Returns an empty degraded result indicating external data is unavailable.</summary>
    public static SpoonacularResult<T> Degraded() =>
        new() { IsAvailable = false, DataSource = DataSource.Degraded };

    /// <summary>Returns an empty degraded result when rate-limited and no cache is available.</summary>
    public static SpoonacularResult<T> RateLimited() =>
        new() { IsAvailable = false, DataSource = DataSource.Degraded, IsLimited = true };
}
