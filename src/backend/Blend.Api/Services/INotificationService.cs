namespace Blend.Api.Services;

/// <summary>
/// Sends in-app notifications to users.
/// </summary>
public interface INotificationService
{
    Task SendIngredientApprovedAsync(
        string recipientUserId,
        string ingredientName,
        CancellationToken cancellationToken = default);

    Task SendIngredientRejectedAsync(
        string recipientUserId,
        string ingredientName,
        string? reason,
        CancellationToken cancellationToken = default);
}
