namespace Blend.Domain.Entities;

/// <summary>
/// Notification sent to a user.
/// Partition key: /recipientUserId
/// TTL: 30 days (2592000 seconds)
/// </summary>
public class Notification : CosmosEntity
{
    public string RecipientUserId { get; set; } = string.Empty;

    public string? SenderUserId { get; set; }

    public NotificationType NotificationType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTimeOffset? ReadAt { get; set; }

    public string? DeepLinkUrl { get; set; }

    public Dictionary<string, string> Payload { get; set; } = [];
}

public enum NotificationType
{
    FriendRequest,
    FriendRequestAccepted,
    RecipeLike,
    RecipeComment,
    RecipeShared,
    SystemAnnouncement,
    CookingReminder,
    WeeklyDigest,
    IngredientSubmissionApproved,
    IngredientSubmissionRejected
}
