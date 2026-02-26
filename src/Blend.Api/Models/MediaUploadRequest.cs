using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models;

public class MediaUploadRequest
{
    [Required]
    public string FileName { get; set; } = "";

    [Required]
    public string ContentType { get; set; } = "";

    [Required]
    public string EntityType { get; set; } = "";

    [Required]
    public Guid EntityId { get; set; }

    /// <summary>File size in bytes, used for pre-validation before blob upload.</summary>
    public long? FileSizeBytes { get; set; }
}
