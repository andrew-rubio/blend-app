using Blend.Api.Recipes.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;

namespace Blend.Api.Recipes.Services;

public enum RecipeOpResult
{
    Success,
    NotFound,
    Forbidden,
    AlreadyLiked,
    NotLiked,
    ValidationFailed,
    ConfirmationRequired,
}

public enum RecipeSortOrder
{
    Newest,
    Oldest,
    MostLiked,
}

public interface IRecipeService
{
    IReadOnlyList<string> ValidateCreateRecipe(CreateRecipeRequest request);
    IReadOnlyList<string> ValidateUpdateRecipe(UpdateRecipeRequest request);

    Task<Recipe> CreateRecipeAsync(string userId, CreateRecipeRequest request, CancellationToken ct = default);
    Task<Recipe?> GetRecipeAsync(string id, string? requestingUserId, CancellationToken ct = default);
    Task<(Recipe? Recipe, RecipeOpResult Result, IReadOnlyList<string>? Errors)> UpdateRecipeAsync(string id, string requestingUserId, UpdateRecipeRequest request, CancellationToken ct = default);
    Task<RecipeOpResult> DeleteRecipeAsync(string id, string requestingUserId, bool confirmed, CancellationToken ct = default);
    Task<(Recipe? Recipe, RecipeOpResult Result)> SetVisibilityAsync(string id, string requestingUserId, bool isPublic, CancellationToken ct = default);
    Task<PagedResult<Recipe>> GetUserRecipesAsync(string userId, string? requestingUserId, FeedPaginationOptions options, RecipeSortOrder sort = RecipeSortOrder.Newest, CancellationToken ct = default);
    Task<RecipeOpResult> LikeRecipeAsync(string recipeId, string userId, CancellationToken ct = default);
    Task<RecipeOpResult> UnlikeRecipeAsync(string recipeId, string userId, CancellationToken ct = default);
    Task<PagedResult<Recipe>> GetLikedRecipesAsync(string userId, FeedPaginationOptions options, CancellationToken ct = default);
    Task<PagedResult<Activity>> GetLikedByAsync(string recipeId, FeedPaginationOptions options, CancellationToken ct = default);
}
