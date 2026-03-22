using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Preferences.Controllers;

/// <summary>
/// Exposes the predefined preference reference lists (cuisines, dish types, diets, intolerances).
/// These endpoints are public — no authentication required.
/// Lists are aligned with Spoonacular's supported categories (PREF-09).
/// </summary>
[ApiController]
[Route("api/v1/preferences")]
public sealed class PreferenceListsController : ControllerBase
{
    /// <summary>Returns the list of supported cuisine types.</summary>
    [HttpGet("cuisines")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetCuisines() => Ok(PreferenceLists.Cuisines);

    /// <summary>Returns the list of supported dish types.</summary>
    [HttpGet("dish-types")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetDishTypes() => Ok(PreferenceLists.DishTypes);

    /// <summary>Returns the list of supported dietary plans.</summary>
    [HttpGet("diets")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetDiets() => Ok(PreferenceLists.Diets);

    /// <summary>Returns the list of supported intolerances.</summary>
    [HttpGet("intolerances")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetIntolerances() => Ok(PreferenceLists.Intolerances);
}
