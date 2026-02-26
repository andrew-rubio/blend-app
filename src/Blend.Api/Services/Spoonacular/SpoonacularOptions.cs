using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Configuration options for the Spoonacular API client.
/// Bind from the "Spoonacular" configuration section.
/// </summary>
public sealed class SpoonacularOptions
{
    public const string SectionName = "Spoonacular";

    /// <summary>Spoonacular API base URL.</summary>
    public string BaseUrl { get; set; } = "https://api.spoonacular.com";

    /// <summary>
    /// API key loaded from configuration/secrets — never hard-code this value.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Fraction (0–1) of daily quota at which a warning is logged. Default 0.80.</summary>
    public double QuotaWarningThreshold { get; set; } = 0.80;

    /// <summary>
    /// Fraction (0–1) of daily quota at which the service switches to cache-only mode. Default 0.95.
    /// </summary>
    public double QuotaCacheOnlyThreshold { get; set; } = 0.95;

    /// <summary>Assumed total daily quota. Spoonacular free tier = 150.</summary>
    public int DailyQuota { get; set; } = 150;

    // TTLs for the two-tier cache (ADR 0009)
    public TimeSpan SearchL1Ttl { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan SearchL2Ttl { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan RecipeL1Ttl { get; set; } = TimeSpan.FromHours(2);
    public TimeSpan RecipeL2Ttl { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan SubstituteL1Ttl { get; set; } = TimeSpan.FromHours(4);
    public TimeSpan SubstituteL2Ttl { get; set; } = TimeSpan.FromDays(30);
}
