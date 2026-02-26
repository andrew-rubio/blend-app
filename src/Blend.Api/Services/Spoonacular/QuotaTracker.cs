using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Tracks Spoonacular API quota usage from X-API-Quota-Used response headers.
/// Thread-safe via Interlocked operations.
/// </summary>
internal sealed class QuotaTracker
{
    private volatile int _quotaUsed;
    private readonly SpoonacularOptions _options;
    private readonly ILogger _logger;

    public QuotaTracker(IOptions<SpoonacularOptions> options, ILogger logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Current quota used (absolute value).</summary>
    public int QuotaUsed => _quotaUsed;

    /// <summary>Current usage as a fraction of the daily quota (0–1).</summary>
    public double UsageFraction =>
        _options.DailyQuota > 0 ? (double)_quotaUsed / _options.DailyQuota : 0;

    /// <summary>True when the quota has crossed the cache-only threshold.</summary>
    public bool IsCacheOnly => UsageFraction >= _options.QuotaCacheOnlyThreshold;

    /// <summary>
    /// Update quota from an X-API-Quota-Used header value.
    /// Logs a warning when the warning threshold is crossed.
    /// </summary>
    public void Update(System.Net.Http.HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-API-Quota-Used", out var values)
            && int.TryParse(values.FirstOrDefault(), out var used))
        {
            Interlocked.Exchange(ref _quotaUsed, used);
            var fraction = UsageFraction;

            if (fraction >= _options.QuotaCacheOnlyThreshold)
            {
                _logger.LogWarning(
                    "Spoonacular quota at {Fraction:P0} ({Used}/{Total}) — switching to cache-only mode",
                    fraction, used, _options.DailyQuota);
            }
            else if (fraction >= _options.QuotaWarningThreshold)
            {
                _logger.LogWarning(
                    "Spoonacular quota at {Fraction:P0} ({Used}/{Total}) — approaching limit",
                    fraction, used, _options.DailyQuota);
            }
        }
    }
}
