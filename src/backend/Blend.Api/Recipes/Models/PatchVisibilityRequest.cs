using System.Text.Json.Serialization;

namespace Blend.Api.Recipes.Models;

public sealed class PatchVisibilityRequest
{
    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; init; }
}
