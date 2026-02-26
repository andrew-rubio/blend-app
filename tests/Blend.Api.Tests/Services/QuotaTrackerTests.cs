using System.Net;
using Blend.Api.Services.Spoonacular;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Blend.Api.Tests.Services;

public class QuotaTrackerTests
{
    private static QuotaTracker CreateTracker(double warningThreshold = 0.80, double cacheOnlyThreshold = 0.95, int dailyQuota = 150)
    {
        var opts = Options.Create(new SpoonacularOptions
        {
            ApiKey = "test",
            QuotaWarningThreshold = warningThreshold,
            QuotaCacheOnlyThreshold = cacheOnlyThreshold,
            DailyQuota = dailyQuota
        });
        return new QuotaTracker(opts, NullLogger.Instance);
    }

    [Fact]
    public void InitialState_HasZeroUsage()
    {
        var tracker = CreateTracker();

        Assert.Equal(0, tracker.QuotaUsed);
        Assert.Equal(0, tracker.UsageFraction);
        Assert.False(tracker.IsCacheOnly);
    }

    [Fact]
    public void Update_WithQuotaUsedHeader_UpdatesUsage()
    {
        var tracker = CreateTracker();
        var response = BuildResponse("120");

        tracker.Update(response);

        Assert.Equal(120, tracker.QuotaUsed);
        Assert.Equal(120.0 / 150.0, tracker.UsageFraction, precision: 5);
    }

    [Fact]
    public void IsCacheOnly_WhenBelowThreshold_ReturnsFalse()
    {
        var tracker = CreateTracker();
        tracker.Update(BuildResponse("100")); // 66%

        Assert.False(tracker.IsCacheOnly);
    }

    [Fact]
    public void IsCacheOnly_WhenAtCacheOnlyThreshold_ReturnsTrue()
    {
        var tracker = CreateTracker(cacheOnlyThreshold: 0.95, dailyQuota: 100);
        tracker.Update(BuildResponse("95")); // exactly 95%

        Assert.True(tracker.IsCacheOnly);
    }

    [Fact]
    public void IsCacheOnly_WhenAboveThreshold_ReturnsTrue()
    {
        var tracker = CreateTracker(cacheOnlyThreshold: 0.95, dailyQuota: 100);
        tracker.Update(BuildResponse("100")); // 100%

        Assert.True(tracker.IsCacheOnly);
    }

    [Fact]
    public void Update_WithNoHeader_DoesNotChangeUsage()
    {
        var tracker = CreateTracker();
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        tracker.Update(response);

        Assert.Equal(0, tracker.QuotaUsed);
    }

    [Fact]
    public void Update_WithInvalidHeaderValue_DoesNotThrow()
    {
        var tracker = CreateTracker();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-API-Quota-Used", "not-a-number");

        var ex = Record.Exception(() => tracker.Update(response));
        Assert.Null(ex);
        Assert.Equal(0, tracker.QuotaUsed);
    }

    [Fact]
    public void UsageFraction_WithZeroDailyQuota_ReturnsZero()
    {
        var tracker = CreateTracker(dailyQuota: 0);

        Assert.Equal(0, tracker.UsageFraction);
    }

    private static HttpResponseMessage BuildResponse(string quotaUsed)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-API-Quota-Used", quotaUsed);
        return response;
    }
}
