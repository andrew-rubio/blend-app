using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// ASP.NET Core health check that reports Spoonacular API quota status.
/// </summary>
public sealed class SpoonacularHealthCheck : IHealthCheck
{
    private readonly SpoonacularQuotaMonitor _quotaMonitor;
    private readonly SpoonacularOptions _options;

    public SpoonacularHealthCheck(
        SpoonacularQuotaMonitor quotaMonitor,
        IOptions<SpoonacularOptions> options)
    {
        _quotaMonitor = quotaMonitor;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var used = _quotaMonitor.QuotaUsed;
        var limit = _options.DailyQuotaLimit;

        var data = new Dictionary<string, object>
        {
            ["quotaUsed"] = used,
            ["quotaLimit"] = limit,
        };

        if (_quotaMonitor.IsAtCacheOnlyThreshold(limit, _options.CacheOnlyAtQuotaPercent))
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Spoonacular quota at or above {_options.CacheOnlyAtQuotaPercent:P0} ({used}/{limit}). Operating in cache-only mode.",
                data: data));
        }

        if (_quotaMonitor.IsAtWarningThreshold(limit, _options.WarnAtQuotaPercent))
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Spoonacular quota above {_options.WarnAtQuotaPercent:P0} ({used}/{limit}).",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Spoonacular quota OK ({used}/{limit}).",
            data: data));
    }
}
