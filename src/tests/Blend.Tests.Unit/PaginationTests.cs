using Blend.Domain.Interfaces;
using Xunit;

namespace Blend.Tests.Unit;

/// <summary>
/// Unit tests for pagination types.
/// </summary>
public class PaginationTests
{
    [Fact]
    public void PagedResult_HasMore_TrueWhenTokenPresent()
    {
        var result = new PagedResult<string>
        {
            Items = ["a", "b"],
            ContinuationToken = "some-token"
        };

        Assert.True(result.HasMore);
    }

    [Fact]
    public void PagedResult_HasMore_FalseWhenNoToken()
    {
        var result = new PagedResult<string>
        {
            Items = ["a"],
            ContinuationToken = null
        };

        Assert.False(result.HasMore);
    }

    [Fact]
    public void PagedResult_Count_ReflectsItemCount()
    {
        var result = new PagedResult<int> { Items = [1, 2, 3] };
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void PaginationOptions_DefaultPageSize_Is20()
    {
        var opts = new PaginationOptions();
        Assert.Equal(20, opts.PageSize);
        Assert.Null(opts.ContinuationToken);
    }

    [Fact]
    public void PaginationOptions_CustomPageSize_IsPreserved()
    {
        var opts = new PaginationOptions { PageSize = 50, ContinuationToken = "cursor-abc" };
        Assert.Equal(50, opts.PageSize);
        Assert.Equal("cursor-abc", opts.ContinuationToken);
    }
}
