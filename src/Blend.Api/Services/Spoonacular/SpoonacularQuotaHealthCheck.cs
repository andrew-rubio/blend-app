using Blend.Api.Services.Spoonacular;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Health check that reports Spoonacular API quota status.
/// Degraded at ≥ 80% usage, Unhealthy at ≥ 95% (cache-only).
/// </summary>
public sealed class SpoonacularQuotaHealthCheck : IHealthCheck
{
    private readonly ISpoonacularService _spoonacular;

    public SpoonacularQuotaHealthCheck(ISpoonacularService spoonacular)
    {
        _spoonacular = spoonacular;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var fraction = _spoonacular.CurrentQuotaUsageFraction;
        var data = new Dictionary<string, object>
        {
            ["quotaUsageFraction"] = fraction,
            ["quotaUsagePercent"] = Math.Round(fraction * 100, 1)
        };

        return Task.FromResult(fraction switch
        {
            >= 0.95 => HealthCheckResult.Unhealthy(
                "Spoonacular quota exhausted — cache-only mode active", data: data),
            >= 0.80 => HealthCheckResult.Degraded(
                "Spoonacular quota approaching limit", data: data),
            _ => HealthCheckResult.Healthy("Spoonacular quota OK", data)
        });
    }
}
