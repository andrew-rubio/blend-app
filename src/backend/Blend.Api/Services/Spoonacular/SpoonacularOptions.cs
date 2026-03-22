namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Configuration options for the Spoonacular API client, bound from the
/// <c>Spoonacular</c> section of application configuration (PLAT-06).
/// </summary>
public sealed class SpoonacularOptions
{
    public const string SectionName = "Spoonacular";

    /// <summary>
    /// The Spoonacular API key. Must be set via configuration — never hardcode this value.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL for the Spoonacular API. Defaults to <c>https://api.spoonacular.com</c>.</summary>
    public string BaseUrl { get; set; } = "https://api.spoonacular.com";

    /// <summary>
    /// Total daily API quota for the configured plan. Free tier = 150 requests/day.
    /// </summary>
    public int DailyQuotaLimit { get; set; } = 150;

    /// <summary>
    /// Quota usage percentage at which a warning is logged (default 80%).
    /// </summary>
    public double WarnAtQuotaPercent { get; set; } = 0.80;

    /// <summary>
    /// Quota usage percentage at which the service switches to cache-only mode (default 95%).
    /// </summary>
    public double CacheOnlyAtQuotaPercent { get; set; } = 0.95;
}
