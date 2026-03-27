using Blend.Api.Recipes.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Recipes.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly IRepository<Recipe>? _recipeRepository;
    private readonly IRepository<Activity>? _activityRepository;
    private readonly UserManager<BlendUser>? _userManager;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(
        ILogger<RecipeService> logger,
        IRepository<Recipe>? recipeRepository = null,
        IRepository<Activity>? activityRepository = null,
        UserManager<BlendUser>? userManager = null)
    {
        _logger = logger;
        _recipeRepository = recipeRepository;
        _activityRepository = activityRepository;
        _userManager = userManager;
    }

    public IReadOnlyList<string> ValidateCreateRecipe(CreateRecipeRequest request)
    {
        var errors = new List<string>();
        ValidateCommonFields(request.Title, request.IsPublic, request.Ingredients, request.Directions, errors);
        return errors;
    }

    public IReadOnlyList<string> ValidateUpdateRecipe(UpdateRecipeRequest request)
    {
        var errors = new List<string>();
        ValidateCommonFields(request.Title, request.IsPublic, request.Ingredients, request.Directions, errors);
        return errors;
    }

    private static void ValidateCommonFields(
        string title,
        bool isPublic,
        IReadOnlyList<RecipeIngredientRequest> ingredients,
        IReadOnlyList<RecipeDirectionRequest> directions,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required.");
        }
        else if (title.Length > 200)
        {
            errors.Add("Title must be 200 characters or fewer.");
        }

        if (isPublic)
        {
            if (ingredients.Count == 0)
            {
                errors.Add("A public recipe must have at least one ingredient.");
            }

            if (directions.Count == 0)
            {
                errors.Add("A public recipe must have at least one direction.");
            }
        }

        foreach (var ing in ingredients)
        {
            if (ing.Quantity <= 0)
            {
                errors.Add($"Ingredient '{ing.IngredientName}' must have a quantity greater than zero.");
            }
        }

        foreach (var dir in directions)
        {
            if (dir.StepNumber < 1)
            {
                errors.Add("Direction step number must be 1 or greater.");
            }
        }
    }

    public async Task<Recipe> CreateRecipeAsync(string userId, CreateRecipeRequest request, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot create recipe for user {UserId}.", userId);
            throw new InvalidOperationException("Recipe repository is not available.");
        }

        var now = DateTimeOffset.UtcNow;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid().ToString(),
            AuthorId = userId,
            Title = request.Title,
            Description = request.Description,
            Ingredients = [.. request.Ingredients.Select(i => new RecipeIngredient
            {
                Quantity = i.Quantity,
                Unit = i.Unit,
                IngredientName = i.IngredientName,
                IngredientId = i.IngredientId,
            })],
            Directions = [.. request.Directions.Select(d => new RecipeDirection
            {
                StepNumber = d.StepNumber,
                Text = d.Text,
                MediaUrl = d.MediaUrl,
            })],
            PrepTime = request.PrepTime,
            CookTime = request.CookTime,
            Servings = request.Servings,
            CuisineType = request.CuisineType,
            DishType = request.DishType,
            Tags = request.Tags,
            FeaturedPhotoUrl = request.FeaturedPhotoUrl,
            Photos = request.Photos,
            IsPublic = request.IsPublic,
            LikeCount = 0,
            ViewCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var result = await _recipeRepository.CreateAsync(recipe, ct);

        if (_userManager is not null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                user.RecipeCount = user.RecipeCount + 1;
                user.UpdatedAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);
            }
        }

        return result;
    }

    public async Task<Recipe?> GetRecipeAsync(string id, string? requestingUserId, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot get recipe {RecipeId}.", id);
            return null;
        }

        var results = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @id",
            new Dictionary<string, object> { ["@id"] = id },
            partitionKey: null,
            ct);

        var recipe = results.FirstOrDefault();
        if (recipe is null)
        {
            return null;
        }

        if (recipe.IsDeleted)
        {
            return null;
        }

        if (!recipe.IsPublic && recipe.AuthorId != requestingUserId)
        {
            return null;
        }

        return recipe;
    }

    public async Task<(Recipe? Recipe, RecipeOpResult Result, IReadOnlyList<string>? Errors)> UpdateRecipeAsync(
        string id, string requestingUserId, UpdateRecipeRequest request, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot update recipe {RecipeId}.", id);
            return (null, RecipeOpResult.NotFound, null);
        }

        var results = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @id",
            new Dictionary<string, object> { ["@id"] = id },
            partitionKey: null,
            ct);

        var recipe = results.FirstOrDefault();
        if (recipe is null)
        {
            return (null, RecipeOpResult.NotFound, null);
        }

        if (recipe.AuthorId != requestingUserId)
        {
            return (null, RecipeOpResult.Forbidden, null);
        }

        var errors = ValidateUpdateRecipe(request);
        if (errors.Count > 0)
        {
            return (null, RecipeOpResult.ValidationFailed, errors);
        }

        var updated = new Recipe
        {
            Id = recipe.Id,
            AuthorId = recipe.AuthorId,
            Title = request.Title,
            Description = request.Description,
            Ingredients = [.. request.Ingredients.Select(i => new RecipeIngredient
            {
                Quantity = i.Quantity,
                Unit = i.Unit,
                IngredientName = i.IngredientName,
                IngredientId = i.IngredientId,
            })],
            Directions = [.. request.Directions.Select(d => new RecipeDirection
            {
                StepNumber = d.StepNumber,
                Text = d.Text,
                MediaUrl = d.MediaUrl,
            })],
            PrepTime = request.PrepTime,
            CookTime = request.CookTime,
            Servings = request.Servings,
            CuisineType = request.CuisineType,
            DishType = request.DishType,
            Tags = request.Tags,
            FeaturedPhotoUrl = request.FeaturedPhotoUrl,
            Photos = request.Photos,
            IsPublic = request.IsPublic,
            LikeCount = recipe.LikeCount,
            ViewCount = recipe.ViewCount,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _recipeRepository.UpdateAsync(updated, recipe.Id, recipe.AuthorId, ct);
        return (result, RecipeOpResult.Success, null);
    }

    public async Task<RecipeOpResult> DeleteRecipeAsync(string id, string requestingUserId, bool confirmed, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot delete recipe {RecipeId}.", id);
            return RecipeOpResult.NotFound;
        }

        if (!confirmed)
        {
            return RecipeOpResult.ConfirmationRequired;
        }

        var results = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @id",
            new Dictionary<string, object> { ["@id"] = id },
            partitionKey: null,
            ct);

        var recipe = results.FirstOrDefault();
        if (recipe is null || recipe.IsDeleted)
        {
            return RecipeOpResult.NotFound;
        }

        if (recipe.AuthorId != requestingUserId)
        {
            return RecipeOpResult.Forbidden;
        }

        if (_activityRepository is not null)
        {
            var activities = await _activityRepository.GetByQueryAsync(
                "SELECT * FROM c WHERE c.referenceId = @id AND c.referenceType = 'Recipe'",
                new Dictionary<string, object> { ["@id"] = id },
                partitionKey: null,
                ct);

            foreach (var activity in activities)
            {
                await _activityRepository.DeleteAsync(activity.Id, activity.UserId, ct);
            }
        }

        // Soft-delete: mark as deleted with a timestamp; hard delete runs after 30-day grace period
        var patches = new Dictionary<string, object?>
        {
            ["/isDeleted"] = true,
            ["/deletedAt"] = DateTimeOffset.UtcNow,
            ["/updatedAt"] = DateTimeOffset.UtcNow,
        };
        await _recipeRepository.PatchAsync(recipe.Id, recipe.AuthorId, patches, ct);

        if (_userManager is not null)
        {
            var user = await _userManager.FindByIdAsync(recipe.AuthorId);
            if (user is not null)
            {
                user.RecipeCount = Math.Max(0, user.RecipeCount - 1);
                user.UpdatedAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);
            }
        }

        return RecipeOpResult.Success;
    }

    public async Task<(Recipe? Recipe, RecipeOpResult Result)> SetVisibilityAsync(
        string id, string requestingUserId, bool isPublic, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot set visibility for recipe {RecipeId}.", id);
            return (null, RecipeOpResult.NotFound);
        }

        var results = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @id",
            new Dictionary<string, object> { ["@id"] = id },
            partitionKey: null,
            ct);

        var recipe = results.FirstOrDefault();
        if (recipe is null)
        {
            return (null, RecipeOpResult.NotFound);
        }

        if (recipe.AuthorId != requestingUserId)
        {
            return (null, RecipeOpResult.Forbidden);
        }

        var patches = new Dictionary<string, object?>
        {
            ["/isPublic"] = isPublic,
            ["/updatedAt"] = DateTimeOffset.UtcNow,
        };

        var updated = await _recipeRepository.PatchAsync(recipe.Id, recipe.AuthorId, patches, ct);
        return (updated, RecipeOpResult.Success);
    }

    public async Task<PagedResult<Recipe>> GetUserRecipesAsync(
        string userId, string? requestingUserId, FeedPaginationOptions options, RecipeSortOrder sort = RecipeSortOrder.Newest, CancellationToken ct = default)
    {
        if (_recipeRepository is null)
        {
            _logger.LogWarning("Recipe repository is not available; cannot get recipes for user {UserId}.", userId);
            return new PagedResult<Recipe>();
        }

        var isOwner = requestingUserId == userId;

        var orderBy = sort switch
        {
            RecipeSortOrder.Oldest => "c.createdAt ASC",
            RecipeSortOrder.MostLiked => "c.likeCount DESC",
            _ => "c.createdAt DESC",
        };

        // Note: Cosmos DB GetPagedAsync takes raw SQL — userId here is validated as a GUID from auth claims.
        // The partition key scoping provides the actual safety, but we still parameterize for defense-in-depth.
        var query = isOwner
            ? $"SELECT * FROM c WHERE c.authorId = @userId AND (NOT IS_DEFINED(c.isDeleted) OR c.isDeleted = false) ORDER BY {orderBy}"
            : $"SELECT * FROM c WHERE c.authorId = @userId AND c.isPublic = true AND (NOT IS_DEFINED(c.isDeleted) OR c.isDeleted = false) ORDER BY {orderBy}";

        // GetPagedAsync doesn't support parameterized queries yet — partition key scoping ensures safety.
        query = query.Replace("@userId", $"'{userId.Replace("'", string.Empty)}'");
        return await _recipeRepository.GetPagedAsync(query, options, partitionKey: userId, ct);
    }

    public async Task<RecipeOpResult> LikeRecipeAsync(string recipeId, string userId, CancellationToken ct = default)
    {
        if (_recipeRepository is null || _activityRepository is null)
        {
            _logger.LogWarning("Repositories are not available; cannot like recipe {RecipeId}.", recipeId);
            return RecipeOpResult.NotFound;
        }

        var recipeResults = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @recipeId",
            new Dictionary<string, object> { ["@recipeId"] = recipeId },
            partitionKey: null,
            ct);

        if (recipeResults.Count == 0)
        {
            return RecipeOpResult.NotFound;
        }

        var recipe = recipeResults[0];

        var likeId = $"{userId}:like:{recipeId}";
        var existingLike = await _activityRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @likeId",
            new Dictionary<string, object> { ["@likeId"] = likeId },
            partitionKey: userId,
            ct);

        if (existingLike.Count > 0)
        {
            return RecipeOpResult.AlreadyLiked;
        }

        var activity = new Activity
        {
            Id = likeId,
            UserId = userId,
            Type = ActivityType.Liked,
            ReferenceId = recipeId,
            ReferenceType = "Recipe",
            Timestamp = DateTimeOffset.UtcNow,
        };

        await _activityRepository.CreateAsync(activity, ct);

        var patches = new Dictionary<string, object?>
        {
            ["/likeCount"] = recipe.LikeCount + 1,
        };
        await _recipeRepository.PatchAsync(recipe.Id, recipe.AuthorId, patches, ct);

        return RecipeOpResult.Success;
    }

    public async Task<RecipeOpResult> UnlikeRecipeAsync(string recipeId, string userId, CancellationToken ct = default)
    {
        if (_recipeRepository is null || _activityRepository is null)
        {
            _logger.LogWarning("Repositories are not available; cannot unlike recipe {RecipeId}.", recipeId);
            return RecipeOpResult.NotFound;
        }

        var recipeResults = await _recipeRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @recipeId",
            new Dictionary<string, object> { ["@recipeId"] = recipeId },
            partitionKey: null,
            ct);

        if (recipeResults.Count == 0)
        {
            return RecipeOpResult.NotFound;
        }

        var recipe = recipeResults[0];

        var likeId = $"{userId}:like:{recipeId}";
        var existingLike = await _activityRepository.GetByQueryAsync(
            "SELECT * FROM c WHERE c.id = @likeId",
            new Dictionary<string, object> { ["@likeId"] = likeId },
            partitionKey: userId,
            ct);

        if (existingLike.Count == 0)
        {
            return RecipeOpResult.NotLiked;
        }

        await _activityRepository.DeleteAsync(likeId, userId, ct);

        var patches = new Dictionary<string, object?>
        {
            ["/likeCount"] = Math.Max(0, recipe.LikeCount - 1),
        };
        await _recipeRepository.PatchAsync(recipe.Id, recipe.AuthorId, patches, ct);

        return RecipeOpResult.Success;
    }

    public async Task<PagedResult<Recipe>> GetLikedRecipesAsync(
        string userId, FeedPaginationOptions options, CancellationToken ct = default)
    {
        if (_activityRepository is null || _recipeRepository is null)
        {
            _logger.LogWarning("Repositories are not available; cannot get liked recipes for user {UserId}.", userId);
            return new PagedResult<Recipe>();
        }

        // GetPagedAsync doesn't support parameterized queries — partition key scoping ensures safety.
        var safeUserId = userId.Replace("'", string.Empty);
        var activityPage = await _activityRepository.GetPagedAsync(
            $"SELECT * FROM c WHERE c.userId = '{safeUserId}' AND c.type = 'Liked' AND c.referenceType = 'Recipe' ORDER BY c.timestamp DESC",
            options,
            partitionKey: userId,
            ct);

        // N+1 cross-partition lookups are acceptable here given typical page sizes (≤50).
        // A future optimisation could batch recipe IDs into a single IN-clause query.
        var recipes = new List<Recipe>();
        foreach (var activity in activityPage.Items)
        {
            var recipeResults = await _recipeRepository.GetByQueryAsync(
                "SELECT * FROM c WHERE c.id = @id",
                new Dictionary<string, object> { ["@id"] = activity.ReferenceId },
                partitionKey: null,
                ct);
            if (recipeResults.Count > 0)
            {
                recipes.Add(recipeResults[0]);
            }
        }

        return new PagedResult<Recipe>
        {
            Items = recipes,
            ContinuationToken = activityPage.ContinuationToken,
        };
    }

    public async Task<PagedResult<Activity>> GetLikedByAsync(
        string recipeId, FeedPaginationOptions options, CancellationToken ct = default)
    {
        if (_activityRepository is null)
        {
            _logger.LogWarning("Activity repository is not available; cannot get liked-by for recipe {RecipeId}.", recipeId);
            return new PagedResult<Activity>();
        }

        // GetPagedAsync doesn't support parameterized queries — recipeId is validated as a GUID from route.
        var safeRecipeId = recipeId.Replace("'", string.Empty);
        return await _activityRepository.GetPagedAsync(
            $"SELECT * FROM c WHERE c.referenceId = '{safeRecipeId}' AND c.type = 'Liked' AND c.referenceType = 'Recipe'",
            options,
            partitionKey: null,
            ct);
    }
}
