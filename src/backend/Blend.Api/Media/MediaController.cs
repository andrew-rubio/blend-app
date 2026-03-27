using System.Security.Claims;
using Blend.Api.Media.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Blend.Infrastructure.BlobStorage;
using Blend.Infrastructure.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blend.Api.Media;

/// <summary>
/// Handles media upload orchestration: SAS-token generation and upload-completion notification.
/// </summary>
[ApiController]
[Route("api/v1/media")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly IBlobStorageService _blobStorage;
    private readonly IImageProcessingService _imageProcessing;
    private readonly BlobStorageOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly IRepository<User>? _userRepository;
    private readonly IRepository<Recipe>? _recipeRepository;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        IBlobStorageService blobStorage,
        IImageProcessingService imageProcessing,
        IOptions<BlobStorageOptions> options,
        IWebHostEnvironment env,
        ILogger<MediaController> logger,
        IRepository<User>? userRepository = null,
        IRepository<Recipe>? recipeRepository = null)
    {
        _blobStorage = blobStorage;
        _imageProcessing = imageProcessing;
        _options = options.Value;
        _env = env;
        _logger = logger;
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    // ── POST /api/v1/media/upload-url ─────────────────────────────────────

    /// <summary>
    /// Generates a time-limited, write-only SAS URL scoped to the specific blob path
    /// for the authenticated user. The browser uses this URL to PUT the file directly
    /// to Azure Blob Storage (or Azurite in development).
    /// </summary>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(UploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult GetUploadUrl([FromBody] UploadUrlRequest request)
    {
        // ── Validate content type (PLAT-27) ────────────────────────────────
        if (!MediaValidation.IsAllowedContentType(request.ContentType))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unsupported media type",
                detail: $"Content type '{request.ContentType}' is not allowed. " +
                        $"Accepted types: {string.Join(", ", MediaValidation.AllowedContentTypes)}.");
        }

        // ── Validate file size (PLAT-28) ───────────────────────────────────
        if (!MediaValidation.IsWithinSizeLimit(request.FileSizeBytes, _options.MaxFileSizeBytes))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "File too large",
                detail: $"File size {request.FileSizeBytes:N0} bytes exceeds the maximum " +
                        $"allowed size of {_options.MaxFileSizeBytes:N0} bytes.");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "User identity could not be resolved from the token.");
        }

        // ── Build a scoped blob path ────────────────────────────────────────
        var extension = GetExtension(request.ContentType);
        var uniqueName = $"{Guid.NewGuid()}{extension}";

        var blobPath = request.UploadUse switch
        {
            MediaUploadUse.Profile => BlobPathBuilder.ForProfile(userId, uniqueName),
            MediaUploadUse.Recipe  => BlobPathBuilder.ForRecipe(request.EntityId, request.RecipeVersion, uniqueName),
            MediaUploadUse.Content => BlobPathBuilder.ForContent(request.EntityId, uniqueName),
            _ => null,
        };

        if (blobPath is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid upload use",
                detail: $"Upload use '{request.UploadUse}' is not recognised.");
        }

        // ── Generate write-only SAS token scoped to the specific blob ──────
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.SasTokenExpiryMinutes);

        Uri sasUri;
        try
        {
            sasUri = _blobStorage.GenerateSasUri(blobPath, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SAS URI for blob path {BlobPath}", blobPath);
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Upload URL generation failed",
                detail: "Unable to generate an upload URL at this time. Please try again.");
        }

        return Ok(new UploadUrlResponse
        {
            SasUrl    = sasUri.ToString(),
            BlobPath  = blobPath,
            ExpiresAt = expiresAt,
        });
    }

    // ── POST /api/v1/media/upload-complete ────────────────────────────────

    /// <summary>
    /// Called by the browser after a successful direct upload to Blob Storage.
    /// In development, image processing is triggered synchronously.
    /// In production, the upload is recorded for asynchronous processing via Azure Function.
    /// </summary>
    [HttpPost("upload-complete")]
    [ProducesResponseType(typeof(UploadCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteUpload(
        [FromBody] UploadCompleteRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "User identity could not be resolved from the token.");
        }

        // Verify the blob was actually uploaded before we do any processing.
        var exists = await _blobStorage.ExistsAsync(request.BlobPath, ct);
        if (!exists)
        {
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Upload not found",
                detail: $"No blob was found at path '{request.BlobPath}'. " +
                        "Ensure the file was uploaded before calling this endpoint.");
        }

        var mediaUrl = _blobStorage.GetPublicUrl(request.BlobPath);
        ProcessingResult? processingResult = null;
        var processingPending = false;

        if (_env.IsDevelopment())
        {
            // Synchronous processing in development
            processingResult = await _imageProcessing.ProcessAsync(request.BlobPath, request.UploadUse, ct);

            if (!processingResult.Success)
            {
                _logger.LogWarning(
                    "Image processing failed for {BlobPath}: {Error}",
                    request.BlobPath, processingResult.ErrorMessage);
                // Original is still accessible; flag it as pending
                processingPending = true;
            }
        }
        else
        {
            // In production the Azure Function BlobTrigger handles processing asynchronously.
            processingPending = true;
            _logger.LogInformation(
                "Upload complete for {BlobPath} (async processing pending)", request.BlobPath);
        }

        // ── Update entity metadata in Cosmos DB ────────────────────────────
        // Use the hero variant URL when available; fall back to the original.
        var primaryUrl = processingResult?.Variants
            .FirstOrDefault(v => v.VariantName is "hero" or "avatar")
            ?.Url ?? mediaUrl;

        await TryUpdateEntityAsync(request.UploadUse, request.EntityId, primaryUrl, ct);

        return Ok(new UploadCompleteResponse
        {
            MediaUrl         = primaryUrl,
            Variants         = processingResult?.Variants ?? [],
            ProcessingPending = processingPending,
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static string GetExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png"  => ".png",
        "image/webp" => ".webp",
        "video/mp4"  => ".mp4",
        _            => string.Empty,
    };

    private async Task TryUpdateEntityAsync(
        MediaUploadUse uploadUse,
        string entityId,
        string mediaUrl,
        CancellationToken ct)
    {
        try
        {
            switch (uploadUse)
            {
                case MediaUploadUse.Profile when _userRepository is not null:
                    await _userRepository.PatchAsync(
                        entityId,
                        entityId,
                        new Dictionary<string, object?> { ["/profilePhotoUrl"] = mediaUrl },
                        ct);
                    break;

                case MediaUploadUse.Recipe when _recipeRepository is not null:
                {
                    // Recipes use authorId as partition key, which is unknown at this point.
                    // Use a cross-partition parameterized query to locate the recipe.
                    var results = await _recipeRepository.GetByQueryAsync(
                        "SELECT * FROM c WHERE c.id = @entityId",
                        new Dictionary<string, object> { ["@entityId"] = entityId },
                        partitionKey: null,
                        ct);
                    var recipe = results.FirstOrDefault();
                    if (recipe is not null)
                    {
                        await _recipeRepository.PatchAsync(
                            entityId,
                            recipe.AuthorId,
                            new Dictionary<string, object?> { ["/featuredPhotoUrl"] = mediaUrl },
                            ct);
                    }
                    break;
                }

                // Content entities updated by admin workflows; no automatic update here.
                case MediaUploadUse.Content:
                    break;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Entity update is non-critical: log and continue, media URL is still returned.
            _logger.LogWarning(ex,
                "Failed to update entity {EntityId} ({UploadUse}) after media upload",
                entityId, uploadUse);
        }
    }
}
