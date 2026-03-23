using Blend.Domain.Entities;
using System.Text.Json.Serialization;

namespace Blend.Domain.Identity;

/// <summary>
/// ASP.NET Core Identity user stored in the Cosmos DB 'users' container.
/// This is the identity document used by the custom <c>CosmosUserStore</c>.
/// </summary>
public sealed class BlendUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Set equal to <see cref="Email"/> — Identity requires UserName to be unique.</summary>
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("normalizedUserName")]
    public string NormalizedUserName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("normalizedEmail")]
    public string NormalizedEmail { get; set; } = string.Empty;

    [JsonPropertyName("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [JsonPropertyName("passwordHash")]
    public string? PasswordHash { get; set; }

    [JsonPropertyName("securityStamp")]
    public string SecurityStamp { get; set; } = string.Empty;

    /// <summary>Set by Identity's UserManager on create/update; initialized to empty here.</summary>
    [JsonPropertyName("concurrencyStamp")]
    public string ConcurrencyStamp { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("role")]
    public UserRole Role { get; set; } = UserRole.User;

    [JsonPropertyName("profilePhotoUrl")]
    public string? ProfilePhotoUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; set; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; set; }

    [JsonPropertyName("followerCount")]
    public int FollowerCount { get; set; }

    [JsonPropertyName("followingCount")]
    public int FollowingCount { get; set; }

    /// <summary>External (social) login providers linked to this account.</summary>
    [JsonPropertyName("externalLogins")]
    public List<ExternalLoginInfo> ExternalLogins { get; set; } = [];
}
