using System.ComponentModel.DataAnnotations;
using Blend.Infrastructure.Media;

namespace Blend.Api.Media.Models;

/// <summary>Request body for <c>POST /api/v1/media/upload-complete</c>.</summary>
public sealed record UploadCompleteRequest
{
    /// <summary>
    /// The blob path returned by the <c>upload-url</c> endpoint, confirming which blob was uploaded.
    /// </summary>
    [Required]
    public string BlobPath { get; init; } = string.Empty;

    /// <summary>Intended use that was specified when generating the SAS URL.</summary>
    [Required]
    public MediaUploadUse UploadUse { get; init; }

    /// <summary>ID of the entity the asset belongs to.</summary>
    [Required]
    public string EntityId { get; init; } = string.Empty;
}
