using Blend.Infrastructure.Media;

namespace Blend.Tests.Unit.Media;

public class MediaValidationTests
{
    // ── Content type validation (PLAT-27) ────────────────────────────────────

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("video/mp4")]
    public void IsAllowedContentType_AllowedType_ReturnsTrue(string contentType)
    {
        Assert.True(MediaValidation.IsAllowedContentType(contentType));
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/avi")]
    [InlineData("")]
    [InlineData("image/bmp")]
    public void IsAllowedContentType_DisallowedType_ReturnsFalse(string contentType)
    {
        Assert.False(MediaValidation.IsAllowedContentType(contentType));
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/Png")]
    [InlineData("VIDEO/MP4")]
    public void IsAllowedContentType_CaseInsensitive_ReturnsTrue(string contentType)
    {
        Assert.True(MediaValidation.IsAllowedContentType(contentType));
    }

    // ── File size validation (PLAT-28) ───────────────────────────────────────

    [Theory]
    [InlineData(1,                10_485_760)]   // 1 byte
    [InlineData(1_048_576,        10_485_760)]   // exactly 1 MB
    [InlineData(10_485_760,       10_485_760)]   // exactly at the limit
    public void IsWithinSizeLimit_ValidSize_ReturnsTrue(long fileSizeBytes, long maxBytes)
    {
        Assert.True(MediaValidation.IsWithinSizeLimit(fileSizeBytes, maxBytes));
    }

    [Theory]
    [InlineData(0,          10_485_760)]   // zero bytes
    [InlineData(-1,         10_485_760)]   // negative
    [InlineData(10_485_761, 10_485_760)]   // one byte over the limit
    [InlineData(20_971_520, 10_485_760)]   // 20 MB (double the limit)
    public void IsWithinSizeLimit_InvalidSize_ReturnsFalse(long fileSizeBytes, long maxBytes)
    {
        Assert.False(MediaValidation.IsWithinSizeLimit(fileSizeBytes, maxBytes));
    }

    // ── AllowedContentTypes set ───────────────────────────────────────────────

    [Fact]
    public void AllowedContentTypes_ContainsExactlyExpectedTypes()
    {
        var expected = new[] { "image/jpeg", "image/png", "image/webp", "video/mp4" };
        Assert.Equal(expected.Length, MediaValidation.AllowedContentTypes.Count);
        foreach (var type in expected)
        {
            Assert.Contains(type, MediaValidation.AllowedContentTypes);
        }
    }
}
