namespace Blend.Api.Ingredients.Models;

/// <summary>Knowledge-base availability statuses (PLAT-50 through PLAT-52).</summary>
public static class KbStatus
{
    public const string Healthy = "healthy";
    public const string Degraded = "degraded";
    public const string Unavailable = "unavailable";
}

/// <summary>
/// Response body for the ingredient knowledge-base health check endpoint.
/// </summary>
public sealed record KbHealthResponse(string Status, DateTimeOffset LastChecked);
