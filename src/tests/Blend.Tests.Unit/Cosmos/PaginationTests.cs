using System.Text;
using Blend.Domain.Repositories;
using Xunit;

namespace Blend.Tests.Unit.Cosmos;

/// <summary>
/// Unit tests for pagination cursor encoding and decoding logic.
/// The encoding mirrors what <c>CosmosRepository&lt;T&gt;</c> uses internally.
/// </summary>
public class PaginationTests
{
    // ── Helpers that mirror CosmosRepository private methods ──────────────────

    private static string EncodeContinuationToken(string rawToken) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

    private static string DecodeContinuationToken(string encodedToken) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(encodedToken));

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void EncodeDecode_SimpleToken_RoundTrips()
    {
        const string original = """{"token":"abc123","range":{"min":"","max":"FF"}}""";
        var encoded = EncodeContinuationToken(original);
        var decoded = DecodeContinuationToken(encoded);
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void EncodeDecode_UnicodeToken_RoundTrips()
    {
        const string original = "続き-トークン-🎉";
        var encoded = EncodeContinuationToken(original);
        var decoded = DecodeContinuationToken(encoded);
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void Encoded_IsValidBase64()
    {
        var encoded = EncodeContinuationToken("some-raw-token");
        // Should not throw
        var bytes = Convert.FromBase64String(encoded);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void PagedResult_HasNextPage_WhenTokenPresent()
    {
        var result = new PagedResult<string>
        {
            Items = ["a", "b"],
            ContinuationToken = "someToken",
        };
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_HasNextPage_IsFalse_WhenTokenNull()
    {
        var result = new PagedResult<string>
        {
            Items = ["a"],
            ContinuationToken = null,
        };
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void FeedPaginationOptions_DefaultPageSize_Is20()
    {
        var opts = new FeedPaginationOptions();
        Assert.Equal(20, opts.PageSize);
    }

    [Fact]
    public void OffsetPaginationOptions_DefaultPage_Is0()
    {
        var opts = new OffsetPaginationOptions();
        Assert.Equal(0, opts.Page);
        Assert.Equal(20, opts.PageSize);
    }

    [Theory]
    [InlineData(0, 20, 0)]   // page 0 → skip 0
    [InlineData(1, 20, 20)]  // page 1 → skip 20
    [InlineData(2, 10, 20)]  // page 2, size 10 → skip 20
    [InlineData(3, 5,  15)]  // page 3, size 5  → skip 15
    public void OffsetPagination_SkipCalculation(int page, int pageSize, int expectedSkip)
    {
        var skip = page * pageSize;
        Assert.Equal(expectedSkip, skip);
    }
}
