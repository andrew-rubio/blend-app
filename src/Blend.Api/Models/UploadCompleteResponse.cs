namespace Blend.Api.Models;

public class UploadCompleteResponse
{
    public string MediaId { get; set; } = "";
    public MediaVariantUrls Urls { get; set; } = new();
}

public class MediaVariantUrls
{
    public string Original { get; set; } = "";
    public string? Hero { get; set; }
    public string? Card { get; set; }
    public string? Thumbnail { get; set; }
    public string? Avatar { get; set; }
}
