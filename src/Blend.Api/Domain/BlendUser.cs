using Microsoft.AspNetCore.Identity;

namespace Blend.Api.Domain;

public class BlendUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ExternalLogin> ExternalLogins { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public string PartitionKey => "users";
}
