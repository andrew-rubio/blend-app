using Blend.Api.Services.Spoonacular;

namespace Blend.Tests.Unit.Spoonacular;

/// <summary>Unit tests for <see cref="SpoonacularQuotaMonitor"/>.</summary>
public class QuotaMonitorTests
{
    private const int DailyLimit = 150;
    private const double WarnPercent = 0.80;
    private const double CacheOnlyPercent = 0.95;

    // ── Warning threshold ─────────────────────────────────────────────────────

    [Fact]
    public void IsAtWarningThreshold_BelowThreshold_ReturnsFalse()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(100); // 66%

        Assert.False(monitor.IsAtWarningThreshold(DailyLimit, WarnPercent));
    }

    [Fact]
    public void IsAtWarningThreshold_AtExactThreshold_ReturnsTrue()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(120); // 80%

        Assert.True(monitor.IsAtWarningThreshold(DailyLimit, WarnPercent));
    }

    [Fact]
    public void IsAtWarningThreshold_AboveThreshold_ReturnsTrue()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(125); // 83%

        Assert.True(monitor.IsAtWarningThreshold(DailyLimit, WarnPercent));
    }

    // ── Cache-only threshold ──────────────────────────────────────────────────

    [Fact]
    public void IsAtCacheOnlyThreshold_BelowThreshold_ReturnsFalse()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(130); // 86%

        Assert.False(monitor.IsAtCacheOnlyThreshold(DailyLimit, CacheOnlyPercent));
    }

    [Fact]
    public void IsAtCacheOnlyThreshold_AtExactThreshold_ReturnsTrue()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(143); // 95.3% (closest int to 95%)

        Assert.True(monitor.IsAtCacheOnlyThreshold(DailyLimit, CacheOnlyPercent));
    }

    [Fact]
    public void IsAtCacheOnlyThreshold_AtFullQuota_ReturnsTrue()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(150); // 100%

        Assert.True(monitor.IsAtCacheOnlyThreshold(DailyLimit, CacheOnlyPercent));
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void QuotaUsed_InitiallyZero()
    {
        var monitor = new SpoonacularQuotaMonitor();
        Assert.Equal(0, monitor.QuotaUsed);
    }

    [Fact]
    public void IsAtWarningThreshold_ZeroQuota_ReturnsFalse()
    {
        var monitor = new SpoonacularQuotaMonitor();
        Assert.False(monitor.IsAtWarningThreshold(DailyLimit, WarnPercent));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_SetsQuotaUsed()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(75);
        Assert.Equal(75, monitor.QuotaUsed);
    }

    [Fact]
    public void Update_OverwritesPreviousValue()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(10);
        monitor.Update(50);
        Assert.Equal(50, monitor.QuotaUsed);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void IsAtWarningThreshold_ZeroLimit_ReturnsFalse()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(100);
        Assert.False(monitor.IsAtWarningThreshold(0, WarnPercent));
    }

    [Fact]
    public void IsAtCacheOnlyThreshold_ZeroLimit_ReturnsFalse()
    {
        var monitor = new SpoonacularQuotaMonitor();
        monitor.Update(100);
        Assert.False(monitor.IsAtCacheOnlyThreshold(0, CacheOnlyPercent));
    }
}
