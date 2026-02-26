namespace Blend.Domain.Entities;

/// <summary>
/// Represents a user account.
/// Partition key: /id
/// </summary>
public class User : CosmosEntity
{
    /// <summary>The partition key path is /id.</summary>
    public string PartitionKey => Id;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public AuthProvider AuthProvider { get; set; }

    public string? ExternalId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastLoginAt { get; set; }

    public UserPreferences? Preferences { get; set; }
}

public enum AuthProvider
{
    Local,
    Google,
    Apple,
    Microsoft
}
