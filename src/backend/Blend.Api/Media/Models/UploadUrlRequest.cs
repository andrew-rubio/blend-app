using System.ComponentModel.DataAnnotations;
using Blend.Infrastructure.Media;

namespace Blend.Api.Media.Models;

/// <summary>Request body for <c>POST /api/v1/media/upload-url</c>.</summary>
public sealed record UploadUrlRequest
{
    /// <summary>MIME type of the file to be uploaded (e.g. <c>image/jpeg</c>).</summary>
    [Required]
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Intended use of the uploaded asset.</summary>
    [Required]
    public MediaUploadUse UploadUse { get; init; }

    /// <summary>ID of the entity the asset belongs to (userId, recipeId, contentId).</summary>
    [Required]
    public string EntityId { get; init; } = string.Empty;

    /// <summary>Intended file size in bytes; used for pre-upload size validation.</summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "FileSizeBytes must be greater than 0.")]
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Optional version string for recipe images (e.g. <c>v1</c>, <c>v2</c>).
    /// Defaults to <c>v1</c> when not provided. Only used when <see cref="UploadUse"/>
    /// is <see cref="MediaUploadUse.Recipe"/>.
    /// </summary>
    public string RecipeVersion { get; init; } = "v1";
}
