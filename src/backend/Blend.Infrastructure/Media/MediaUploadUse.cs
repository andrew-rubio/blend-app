namespace Blend.Infrastructure.Media;

/// <summary>Identifies the intended purpose of a media upload.</summary>
public enum MediaUploadUse
{
    /// <summary>User profile photo (generates an Avatar 200×200 variant).</summary>
    Profile,

    /// <summary>Recipe photo (generates Hero, Card, and Thumbnail variants).</summary>
    Recipe,

    /// <summary>Admin content image (generates Hero and Thumbnail variants).</summary>
    Content,
}
