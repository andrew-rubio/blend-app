using Blend.Api.Admin.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;

namespace Blend.Api.Admin.Services;

/// <summary>
/// Provides admin CRUD operations for featured recipes, stories, videos, and the
/// ingredient submission approval queue (PLAT-25 through PLAT-34).
/// </summary>
public interface IAdminContentService
{
    // ── Featured Recipes ──────────────────────────────────────────────────────

    /// <summary>Returns all featured recipe entries, ordered by <see cref="Content.DisplayOrder"/>.</summary>
    Task<IReadOnlyList<ContentResponse>> GetFeaturedRecipesAsync(CancellationToken ct = default);

    /// <summary>Creates a new featured recipe entry.</summary>
    Task<ContentResponse> CreateFeaturedRecipeAsync(CreateFeaturedRecipeRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing featured recipe entry. Returns <c>null</c> when not found.</summary>
    Task<ContentResponse?> UpdateFeaturedRecipeAsync(string id, UpdateFeaturedRecipeRequest request, CancellationToken ct = default);

    /// <summary>Deletes a content item by id. Returns <c>false</c> when not found.</summary>
    Task<bool> DeleteContentAsync(string id, ContentType contentType, CancellationToken ct = default);

    // ── Stories ───────────────────────────────────────────────────────────────

    /// <summary>Returns all stories, ordered by <see cref="Content.DisplayOrder"/>.</summary>
    Task<IReadOnlyList<ContentResponse>> GetStoriesAsync(CancellationToken ct = default);

    /// <summary>Creates a new story.</summary>
    Task<ContentResponse> CreateStoryAsync(CreateStoryRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing story. Returns <c>null</c> when not found.</summary>
    Task<ContentResponse?> UpdateStoryAsync(string id, UpdateStoryRequest request, CancellationToken ct = default);

    // ── Videos ────────────────────────────────────────────────────────────────

    /// <summary>Returns all videos, ordered by <see cref="Content.DisplayOrder"/>.</summary>
    Task<IReadOnlyList<ContentResponse>> GetVideosAsync(CancellationToken ct = default);

    /// <summary>Creates a new video entry.</summary>
    Task<ContentResponse> CreateVideoAsync(CreateVideoRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing video. Returns <c>null</c> when not found.</summary>
    Task<ContentResponse?> UpdateVideoAsync(string id, UpdateVideoRequest request, CancellationToken ct = default);

    // ── Ingredient Submission Queue ────────────────────────────────────────────

    /// <summary>
    /// Returns a paged list of ingredient submissions, filtered by status.
    /// </summary>
    Task<AdminSubmissionsPageResponse> GetIngredientSubmissionsAsync(
        string? status,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Approves an ingredient submission: updates its status, indexes it in the
    /// Knowledge Base, and notifies the submitting user.
    /// Returns <c>null</c> when the submission is not found.
    /// </summary>
    Task<AdminSubmissionResponse?> ApproveIngredientSubmissionAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Rejects an ingredient submission with an optional reason, and notifies the submitting user.
    /// Returns <c>null</c> when the submission is not found.
    /// </summary>
    Task<AdminSubmissionResponse?> RejectIngredientSubmissionAsync(string id, string? reason, CancellationToken ct = default);
}
