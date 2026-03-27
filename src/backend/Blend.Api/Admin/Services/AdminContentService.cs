using Blend.Api.Admin.Models;
using Blend.Api.Ingredients.Services;
using Blend.Api.Notifications.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Admin.Services;

/// <summary>
/// Implements admin CRUD for featured recipes, stories, videos, and the
/// ingredient submission approval queue (PLAT-25 through PLAT-34).
/// </summary>
public sealed class AdminContentService : IAdminContentService
{
    private readonly IRepository<Content>? _contentRepo;
    private readonly IKnowledgeBaseService? _kbService;
    private readonly INotificationService? _notificationService;
    private readonly ILogger<AdminContentService> _logger;

    public AdminContentService(
        ILogger<AdminContentService> logger,
        IRepository<Content>? contentRepo = null,
        IKnowledgeBaseService? kbService = null,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _contentRepo = contentRepo;
        _kbService = kbService;
        _notificationService = notificationService;
    }

    // ── Featured Recipes ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentResponse>> GetFeaturedRecipesAsync(CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return [];
        }

        var query = "SELECT * FROM c WHERE c.contentType = 'FeaturedRecipe' ORDER BY c.displayOrder ASC";
        var items = await _contentRepo.GetByQueryAsync(query, null, ct);
        return items.Select(ContentResponse.FromEntity).ToList();
    }

    /// <inheritdoc/>
    public async Task<ContentResponse> CreateFeaturedRecipeAsync(
        CreateFeaturedRecipeRequest request,
        CancellationToken ct = default)
    {
        var content = new Content
        {
            Id = Guid.NewGuid().ToString(),
            ContentType = ContentType.FeaturedRecipe,
            Title = request.Title.Trim(),
            Body = request.Description?.Trim(),
            ThumbnailUrl = request.ImageUrl,
            RecipeId = request.RecipeId,
            Source = request.Source,
            DisplayOrder = request.DisplayOrder,
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        if (_contentRepo is null)
        {
            _logger.LogWarning("Content repository unavailable; returning unsaved featured recipe.");
            return ContentResponse.FromEntity(content);
        }

        var created = await _contentRepo.CreateAsync(content, ct);
        return ContentResponse.FromEntity(created);
    }

    /// <inheritdoc/>
    public async Task<ContentResponse?> UpdateFeaturedRecipeAsync(
        string id,
        UpdateFeaturedRecipeRequest request,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return null;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != ContentType.FeaturedRecipe)
        {
            return null;
        }

        var updated = new Content
        {
            Id = existing.Id,
            ContentType = ContentType.FeaturedRecipe,
            Title = request.Title?.Trim() ?? existing.Title,
            Body = request.Description?.Trim() ?? existing.Body,
            ThumbnailUrl = request.ImageUrl ?? existing.ThumbnailUrl,
            RecipeId = request.RecipeId ?? existing.RecipeId,
            Source = request.Source ?? existing.Source,
            DisplayOrder = request.DisplayOrder ?? existing.DisplayOrder,
            IsPublished = existing.IsPublished,
            PublishedAt = existing.PublishedAt,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _contentRepo.UpdateAsync(updated, id, id, ct);
        return ContentResponse.FromEntity(result);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteContentAsync(string id, ContentType contentType, CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return false;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != contentType)
        {
            return false;
        }

        await _contentRepo.DeleteAsync(id, id, ct);
        return true;
    }

    // ── Stories ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentResponse>> GetStoriesAsync(CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return [];
        }

        var query = "SELECT * FROM c WHERE c.contentType = 'Story' ORDER BY c.displayOrder ASC";
        var items = await _contentRepo.GetByQueryAsync(query, null, ct);
        return items.Select(ContentResponse.FromEntity).ToList();
    }

    /// <inheritdoc/>
    public async Task<ContentResponse> CreateStoryAsync(CreateStoryRequest request, CancellationToken ct = default)
    {
        var content = new Content
        {
            Id = Guid.NewGuid().ToString(),
            ContentType = ContentType.Story,
            Title = request.Title.Trim(),
            Body = request.Content?.Trim(),
            ThumbnailUrl = request.CoverImageUrl,
            AuthorName = request.Author?.Trim(),
            RelatedRecipeIds = request.RelatedRecipeIds,
            ReadingTimeMinutes = request.ReadingTimeMinutes,
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        if (_contentRepo is null)
        {
            _logger.LogWarning("Content repository unavailable; returning unsaved story.");
            return ContentResponse.FromEntity(content);
        }

        var created = await _contentRepo.CreateAsync(content, ct);
        return ContentResponse.FromEntity(created);
    }

    /// <inheritdoc/>
    public async Task<ContentResponse?> UpdateStoryAsync(
        string id,
        UpdateStoryRequest request,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return null;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != ContentType.Story)
        {
            return null;
        }

        var updated = new Content
        {
            Id = existing.Id,
            ContentType = ContentType.Story,
            Title = request.Title?.Trim() ?? existing.Title,
            Body = request.Content?.Trim() ?? existing.Body,
            ThumbnailUrl = request.CoverImageUrl ?? existing.ThumbnailUrl,
            AuthorName = request.Author?.Trim() ?? existing.AuthorName,
            RelatedRecipeIds = request.RelatedRecipeIds ?? existing.RelatedRecipeIds,
            ReadingTimeMinutes = request.ReadingTimeMinutes ?? existing.ReadingTimeMinutes,
            IsPublished = existing.IsPublished,
            PublishedAt = existing.PublishedAt,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _contentRepo.UpdateAsync(updated, id, id, ct);
        return ContentResponse.FromEntity(result);
    }

    // ── Videos ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentResponse>> GetVideosAsync(CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return [];
        }

        var query = "SELECT * FROM c WHERE c.contentType = 'Video' ORDER BY c.displayOrder ASC";
        var items = await _contentRepo.GetByQueryAsync(query, null, ct);
        return items.Select(ContentResponse.FromEntity).ToList();
    }

    /// <inheritdoc/>
    public async Task<ContentResponse> CreateVideoAsync(CreateVideoRequest request, CancellationToken ct = default)
    {
        var content = new Content
        {
            Id = Guid.NewGuid().ToString(),
            ContentType = ContentType.Video,
            Title = request.Title.Trim(),
            ThumbnailUrl = request.ThumbnailUrl,
            MediaUrl = request.VideoUrl,
            AuthorName = request.Creator?.Trim(),
            DurationSeconds = request.DurationSeconds,
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        if (_contentRepo is null)
        {
            _logger.LogWarning("Content repository unavailable; returning unsaved video.");
            return ContentResponse.FromEntity(content);
        }

        var created = await _contentRepo.CreateAsync(content, ct);
        return ContentResponse.FromEntity(created);
    }

    /// <inheritdoc/>
    public async Task<ContentResponse?> UpdateVideoAsync(
        string id,
        UpdateVideoRequest request,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return null;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != ContentType.Video)
        {
            return null;
        }

        var updated = new Content
        {
            Id = existing.Id,
            ContentType = ContentType.Video,
            Title = request.Title?.Trim() ?? existing.Title,
            ThumbnailUrl = request.ThumbnailUrl ?? existing.ThumbnailUrl,
            MediaUrl = request.VideoUrl ?? existing.MediaUrl,
            AuthorName = request.Creator?.Trim() ?? existing.AuthorName,
            DurationSeconds = request.DurationSeconds ?? existing.DurationSeconds,
            IsPublished = existing.IsPublished,
            PublishedAt = existing.PublishedAt,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _contentRepo.UpdateAsync(updated, id, id, ct);
        return ContentResponse.FromEntity(result);
    }

    // ── Ingredient Submission Queue ────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<AdminSubmissionsPageResponse> GetIngredientSubmissionsAsync(
        string? status,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return new AdminSubmissionsPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var statusFilter = status?.ToLowerInvariant() switch
        {
            "pending" => "AND c.submissionStatus = 'Pending'",
            "approved" => "AND c.submissionStatus = 'Approved'",
            "rejected" => "AND c.submissionStatus = 'Rejected'",
            _ => string.Empty,
        };

        var query = $"SELECT * FROM c WHERE c.contentType = 'IngredientSubmission' {statusFilter} ORDER BY c.createdAt ASC";

        var options = new FeedPaginationOptions
        {
            PageSize = clampedSize,
            ContinuationToken = cursor,
        };

        var page = await _contentRepo.GetPagedAsync(query, options, null, ct);

        return new AdminSubmissionsPageResponse
        {
            Items = page.Items.Select(AdminSubmissionResponse.FromEntity).ToList(),
            NextCursor = page.ContinuationToken,
            HasMore = page.HasNextPage,
        };
    }

    /// <inheritdoc/>
    public async Task<AdminSubmissionResponse?> ApproveIngredientSubmissionAsync(
        string id,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return null;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != ContentType.IngredientSubmission)
        {
            return null;
        }

        var updated = new Content
        {
            Id = existing.Id,
            ContentType = ContentType.IngredientSubmission,
            Title = existing.Title,
            Body = existing.Body,
            Category = existing.Category,
            SubmittedByUserId = existing.SubmittedByUserId,
            SubmissionStatus = SubmissionStatus.Approved,
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _contentRepo.UpdateAsync(updated, id, id, ct);

        // Index in Knowledge Base
        if (_kbService is not null)
        {
            await _kbService.IndexIngredientAsync(id, existing.Title, existing.Category, ct);
        }

        // Notify submitting user
        if (_notificationService is not null && !string.IsNullOrWhiteSpace(existing.SubmittedByUserId))
        {
            await _notificationService.CreateNotificationAsync(
                existing.SubmittedByUserId,
                NotificationType.IngredientApproved,
                "Ingredient approved",
                $"Your ingredient submission '{existing.Title}' has been approved and added to the knowledge base.",
                ct: ct);
        }

        return AdminSubmissionResponse.FromEntity(result);
    }

    /// <inheritdoc/>
    public async Task<AdminSubmissionResponse?> RejectIngredientSubmissionAsync(
        string id,
        string? reason,
        CancellationToken ct = default)
    {
        if (_contentRepo is null)
        {
            return null;
        }

        var existing = await _contentRepo.GetByIdAsync(id, id, ct);
        if (existing is null || existing.ContentType != ContentType.IngredientSubmission)
        {
            return null;
        }

        var updated = new Content
        {
            Id = existing.Id,
            ContentType = ContentType.IngredientSubmission,
            Title = existing.Title,
            Body = existing.Body,
            Category = existing.Category,
            SubmittedByUserId = existing.SubmittedByUserId,
            SubmissionStatus = SubmissionStatus.Rejected,
            RejectionReason = reason?.Trim(),
            IsPublished = false,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _contentRepo.UpdateAsync(updated, id, id, ct);

        // Notify submitting user
        if (_notificationService is not null && !string.IsNullOrWhiteSpace(existing.SubmittedByUserId))
        {
            var message = string.IsNullOrWhiteSpace(reason)
                ? $"Your ingredient submission '{existing.Title}' has been rejected."
                : $"Your ingredient submission '{existing.Title}' has been rejected. Reason: {reason}";

            await _notificationService.CreateNotificationAsync(
                existing.SubmittedByUserId,
                NotificationType.IngredientRejected,
                "Ingredient submission rejected",
                message,
                ct: ct);
        }

        return AdminSubmissionResponse.FromEntity(result);
    }
}
