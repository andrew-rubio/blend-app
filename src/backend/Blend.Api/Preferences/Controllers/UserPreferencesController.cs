using System.Security.Claims;
using Blend.Api.Preferences.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Preferences.Controllers;

/// <summary>
/// Manages the authenticated user's saved preferences.
/// All endpoints require a valid JWT.
/// </summary>
[ApiController]
[Route("api/v1/users/me/preferences")]
[Authorize]
public sealed class UserPreferencesController : ControllerBase
{
    // JSON Pointer paths used for Cosmos DB patch operations
    private const string PatchPathFavoriteCuisines = "/preferences/favoriteCuisines";
    private const string PatchPathFavoriteDishTypes = "/preferences/favoriteDishTypes";
    private const string PatchPathDiets = "/preferences/diets";
    private const string PatchPathIntolerances = "/preferences/intolerances";
    private const string PatchPathDislikedIngredientIds = "/preferences/dislikedIngredientIds";
    private const string PatchPathUpdatedAt = "/updatedAt";

    private readonly IRepository<User>? _userRepository;
    private readonly ILogger<UserPreferencesController> _logger;

    public UserPreferencesController(
        ILogger<UserPreferencesController> logger,
        IRepository<User>? userRepository = null)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    // ── GET /api/v1/users/me/preferences ─────────────────────────────────────

    /// <summary>Retrieves the current user's saved preferences.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_userRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var user = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (user is null)
        {
            return NotFoundProblem(userId);
        }

        return Ok(user.Preferences);
    }

    // ── PUT /api/v1/users/me/preferences ─────────────────────────────────────

    /// <summary>
    /// Replaces all of the current user's preferences atomically.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_userRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var validationError = ValidatePreferenceValues(
            request.FavoriteCuisines,
            request.FavoriteDishTypes,
            request.Diets,
            request.Intolerances);

        if (validationError is not null)
        {
            return validationError;
        }

        var user = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (user is null)
        {
            return NotFoundProblem(userId);
        }

        var newPreferences = new UserPreferences
        {
            FavoriteCuisines = request.FavoriteCuisines,
            FavoriteDishTypes = request.FavoriteDishTypes,
            Diets = request.Diets,
            Intolerances = request.Intolerances,
            DislikedIngredientIds = request.DislikedIngredientIds,
        };

        var updatedUser = await _userRepository.UpdateAsync(
            new User
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                PasswordHashRef = user.PasswordHashRef,
                Preferences = newPreferences,
                MeasurementUnit = user.MeasurementUnit,
                CreatedAt = user.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                UnreadNotificationCount = user.UnreadNotificationCount,
                Role = user.Role,
            },
            userId,
            userId,
            ct);

        return Ok(updatedUser.Preferences);
    }

    // ── PATCH /api/v1/users/me/preferences ───────────────────────────────────

    /// <summary>
    /// Partially updates the current user's preferences. Only non-null fields are updated.
    /// Uses Cosmos DB patch operations for atomic partial updates.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(typeof(UserPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> PatchPreferences(
        [FromBody] PatchPreferencesRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_userRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var validationError = ValidatePreferenceValues(
            request.FavoriteCuisines,
            request.FavoriteDishTypes,
            request.Diets,
            request.Intolerances);

        if (validationError is not null)
        {
            return validationError;
        }

        var patches = BuildPatches(request);
        if (patches.Count == 0)
        {
            // Nothing to update — return current preferences
            var existingUser = await _userRepository.GetByIdAsync(userId, userId, ct);
            if (existingUser is null)
            {
                return NotFoundProblem(userId);
            }

            return Ok(existingUser.Preferences);
        }

        // Always update the updatedAt timestamp alongside preference patches
        patches[PatchPathUpdatedAt] = DateTimeOffset.UtcNow;

        var updated = await _userRepository.PatchAsync(userId, userId, patches, ct);
        return Ok(updated.Preferences);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service unavailable",
            detail: "The user data store is not available.");

    private IActionResult NotFoundProblem(string userId)
    {
        _logger.LogWarning("User {UserId} not found when accessing preferences.", userId);
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not found",
            detail: "User profile not found.");
    }

    /// <summary>
    /// Validates that all submitted preference values belong to the predefined lists.
    /// Returns a 400 Problem result if any values are invalid, otherwise null.
    /// </summary>
    private IActionResult? ValidatePreferenceValues(
        IReadOnlyList<string>? cuisines,
        IReadOnlyList<string>? dishTypes,
        IReadOnlyList<string>? diets,
        IReadOnlyList<string>? intolerances)
    {
        var errors = new List<string>();

        if (cuisines is not null)
        {
            var invalid = PreferenceLists.GetInvalidCuisines(cuisines);
            if (invalid.Count > 0)
            {
                errors.Add($"Invalid cuisines: {string.Join(", ", invalid)}.");
            }
        }

        if (dishTypes is not null)
        {
            var invalid = PreferenceLists.GetInvalidDishTypes(dishTypes);
            if (invalid.Count > 0)
            {
                errors.Add($"Invalid dish types: {string.Join(", ", invalid)}.");
            }
        }

        if (diets is not null)
        {
            var invalid = PreferenceLists.GetInvalidDiets(diets);
            if (invalid.Count > 0)
            {
                errors.Add($"Invalid diets: {string.Join(", ", invalid)}.");
            }
        }

        if (intolerances is not null)
        {
            var invalid = PreferenceLists.GetInvalidIntolerances(intolerances);
            if (invalid.Count > 0)
            {
                errors.Add($"Invalid intolerances: {string.Join(", ", invalid)}.");
            }
        }

        if (errors.Count == 0)
        {
            return null;
        }

        return Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation failed",
            detail: string.Join(" ", errors));
    }

    /// <summary>Builds the Cosmos DB patch dictionary from a PATCH request's non-null fields.</summary>
    private static Dictionary<string, object?> BuildPatches(PatchPreferencesRequest request)
    {
        var patches = new Dictionary<string, object?>();

        if (request.FavoriteCuisines is not null)
        {
            patches[PatchPathFavoriteCuisines] = request.FavoriteCuisines;
        }

        if (request.FavoriteDishTypes is not null)
        {
            patches[PatchPathFavoriteDishTypes] = request.FavoriteDishTypes;
        }

        if (request.Diets is not null)
        {
            patches[PatchPathDiets] = request.Diets;
        }

        if (request.Intolerances is not null)
        {
            patches[PatchPathIntolerances] = request.Intolerances;
        }

        if (request.DislikedIngredientIds is not null)
        {
            patches[PatchPathDislikedIngredientIds] = request.DislikedIngredientIds;
        }

        return patches;
    }
}
