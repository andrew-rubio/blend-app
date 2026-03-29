using System.Security.Claims;
using Blend.Api.Recipes.Models;
using Blend.Api.Recipes.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Recipes.Controllers;

[ApiController]
[Route("api/v1/recipes")]
[Authorize]
public sealed class RecipesController : ControllerBase
{
    private readonly IRecipeService? _recipeService;
    private readonly UserManager<BlendUser>? _userManager;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(
        ILogger<RecipesController> logger,
        IRecipeService? recipeService = null,
        UserManager<BlendUser>? userManager = null)
    {
        _logger = logger;
        _recipeService = recipeService;
        _userManager = userManager;
    }

    // POST /api/v1/recipes
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var errors = _recipeService.ValidateCreateRecipe(request);
        if (errors.Count > 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: string.Join(" ", errors));
        }

        var recipe = await _recipeService.CreateRecipeAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
    }

    // GET /api/v1/recipes/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecipe(string id, CancellationToken ct)
    {
        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var recipe = await _recipeService.GetRecipeAsync(id, userId, ct);
        if (recipe is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Recipe not found.");
        }

        return Ok(await MapToResponseAsync(recipe, userId, ct));
    }

    // PUT /api/v1/recipes/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateRecipe(string id, [FromBody] UpdateRecipeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var (recipe, result, errors) = await _recipeService.UpdateRecipeAsync(id, userId, request, ct);

        return result switch
        {
            RecipeOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Recipe not found."),
            RecipeOpResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden", detail: "You do not have permission to update this recipe."),
            RecipeOpResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed", detail: string.Join(" ", errors ?? [])),
            _ => Ok(recipe),
        };
    }

    // DELETE /api/v1/recipes/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteRecipe(string id, [FromQuery] bool confirm = false, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _recipeService.DeleteRecipeAsync(id, userId, confirm, ct);

        return result switch
        {
            RecipeOpResult.ConfirmationRequired => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Confirmation required", detail: "Pass ?confirm=true to confirm deletion."),
            RecipeOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Recipe not found."),
            RecipeOpResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden", detail: "You do not have permission to delete this recipe."),
            _ => NoContent(),
        };
    }

    // PATCH /api/v1/recipes/{id}/visibility
    [HttpPatch("{id}/visibility")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetVisibility(string id, [FromBody] PatchVisibilityRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var (recipe, result) = await _recipeService.SetVisibilityAsync(id, userId, request.IsPublic, ct);

        return result switch
        {
            RecipeOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Recipe not found."),
            RecipeOpResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden", detail: "You do not have permission to update this recipe."),
            _ => Ok(recipe),
        };
    }

    // POST /api/v1/recipes/{id}/like
    [HttpPost("{id}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> LikeRecipe(string id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _recipeService.LikeRecipeAsync(id, userId, ct);

        return result switch
        {
            RecipeOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Recipe not found."),
            RecipeOpResult.AlreadyLiked => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Conflict", detail: "You have already liked this recipe."),
            _ => Ok(),
        };
    }

    // DELETE /api/v1/recipes/{id}/like
    [HttpDelete("{id}/like")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UnlikeRecipe(string id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _recipeService.UnlikeRecipeAsync(id, userId, ct);

        return result switch
        {
            RecipeOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Recipe not found."),
            RecipeOpResult.NotLiked => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Conflict", detail: "You have not liked this recipe."),
            _ => NoContent(),
        };
    }

    // GET /api/v1/recipes/{id}/liked-by
    [HttpGet("{id}/liked-by")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetLikedBy(
        string id,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedPageSize = Math.Clamp(pageSize, 1, 50);
        var options = new FeedPaginationOptions { PageSize = clampedPageSize, ContinuationToken = continuationToken };
        var result = await _recipeService.GetLikedByAsync(id, options, ct);
        return Ok(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The recipe service is not available.");

    private async Task<RecipeResponse> MapToResponseAsync(Recipe recipe, string? requestingUserId, CancellationToken ct)
    {
        // Determine data source: Spoonacular recipes have no AuthorId.
        var isSpoonacular = string.IsNullOrEmpty(recipe.AuthorId);

        AuthorResponse? author = null;
        if (!isSpoonacular && _userManager is not null)
        {
            var user = await _userManager.FindByIdAsync(recipe.AuthorId);
            if (user is not null)
            {
                author = new AuthorResponse
                {
                    Id = user.Id,
                    Name = user.DisplayName ?? user.UserName ?? "Unknown",
                    AvatarUrl = user.ProfilePhotoUrl,
                };
            }
        }

        return new RecipeResponse
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImageUrl = recipe.FeaturedPhotoUrl,
            Photos = recipe.Photos,
            Cuisines = recipe.CuisineType is not null ? [recipe.CuisineType] : [],
            DishTypes = recipe.DishType is not null ? [recipe.DishType] : [],
            Diets = recipe.Tags,
            Intolerances = [],
            ReadyInMinutes = recipe.PrepTime + recipe.CookTime > 0 ? recipe.PrepTime + recipe.CookTime : null,
            PrepTimeMinutes = recipe.PrepTime > 0 ? recipe.PrepTime : null,
            CookTimeMinutes = recipe.CookTime > 0 ? recipe.CookTime : null,
            Servings = recipe.Servings,
            Ingredients = recipe.Ingredients.Select((ing, i) => new IngredientResponse
            {
                Id = ing.IngredientId ?? i.ToString(),
                Name = ing.IngredientName,
                Amount = ing.Quantity,
                Unit = ing.Unit,
            }).ToList(),
            Steps = recipe.Directions.Select(d => new StepResponse
            {
                Number = d.StepNumber,
                Step = d.Text,
            }).ToList(),
            DataSource = isSpoonacular ? "Spoonacular" : "Community",
            Author = author,
            NutritionInfo = recipe.NutritionInfo is not null ? new NutritionResponse
            {
                Calories = recipe.NutritionInfo.Calories,
                Protein = recipe.NutritionInfo.Protein,
                Carbs = recipe.NutritionInfo.Carbs,
                Fat = recipe.NutritionInfo.Fat,
                Fiber = recipe.NutritionInfo.Fiber,
                Sugar = recipe.NutritionInfo.Sugar,
            } : null,
            LikeCount = recipe.LikeCount,
            ViewCount = recipe.ViewCount,
            CreatedAt = recipe.CreatedAt != DateTimeOffset.MinValue ? recipe.CreatedAt : null,
            UpdatedAt = recipe.UpdatedAt != DateTimeOffset.MinValue ? recipe.UpdatedAt : null,
        };
    }
}
