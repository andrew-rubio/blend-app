namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Tracks Spoonacular API quota consumption using the <c>X-API-Quota-Used</c> response header.
/// This class is registered as a singleton so quota state is shared across all requests.
/// </summary>
public sealed class SpoonacularQuotaMonitor
{
    private volatile int _quotaUsed;

    /// <summary>The most recently observed quota-used value.</summary>
    public int QuotaUsed => _quotaUsed;

    /// <summary>Updates the quota-used counter from the latest API response header.</summary>
    public void Update(int quotaUsed)
    {
        Interlocked.Exchange(ref _quotaUsed, quotaUsed);
    }

    /// <summary>
    /// Returns true when the warning threshold has been reached (PLAT-07).
    /// </summary>
    public bool IsAtWarningThreshold(int dailyQuotaLimit, double warnAtPercent) =>
        dailyQuotaLimit > 0 && _quotaUsed >= dailyQuotaLimit * warnAtPercent;

    /// <summary>
    /// Returns true when the service must switch to cache-only mode (PLAT-08).
    /// </summary>
    public bool IsAtCacheOnlyThreshold(int dailyQuotaLimit, double cacheOnlyAtPercent) =>
        dailyQuotaLimit > 0 && _quotaUsed >= dailyQuotaLimit * cacheOnlyAtPercent;
}
