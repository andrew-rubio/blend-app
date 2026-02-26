using Blend.Domain.Entities;
using Blend.Domain.Interfaces;

namespace Blend.Api.Services;

/// <summary>
/// Creates in-app notification documents in Cosmos DB.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IRepository<Notification> notificationRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task SendIngredientApprovedAsync(
        string recipientUserId,
        string ingredientName,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            NotificationType = NotificationType.IngredientSubmissionApproved,
            Title = "Ingredient approved!",
            Body = $"Your submission '{ingredientName}' has been approved and added to the knowledge base.",
            Payload = new Dictionary<string, string>
            {
                ["ingredientName"] = ingredientName
            }
        };

        await _notificationRepository.CreateAsync(notification, cancellationToken);
        _logger.LogInformation("Sent approval notification to user {UserId} for ingredient '{Ingredient}'",
            recipientUserId, ingredientName);
    }

    public async Task SendIngredientRejectedAsync(
        string recipientUserId,
        string ingredientName,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var body = string.IsNullOrWhiteSpace(reason)
            ? $"Your submission '{ingredientName}' was not approved."
            : $"Your submission '{ingredientName}' was not approved. Reason: {reason}";

        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            NotificationType = NotificationType.IngredientSubmissionRejected,
            Title = "Ingredient submission not approved",
            Body = body,
            Payload = new Dictionary<string, string>
            {
                ["ingredientName"] = ingredientName
            }
        };

        await _notificationRepository.CreateAsync(notification, cancellationToken);
        _logger.LogInformation("Sent rejection notification to user {UserId} for ingredient '{Ingredient}'",
            recipientUserId, ingredientName);
    }
}
