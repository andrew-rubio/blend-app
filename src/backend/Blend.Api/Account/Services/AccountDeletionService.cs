using Blend.Api.Account.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Account.Services;

/// <summary>
/// Implements account deletion lifecycle: request → 30-day grace → permanent deletion
/// (SETT-17 through SETT-24, REQ-61).
/// </summary>
public sealed class AccountDeletionService : IAccountDeletionService
{
    /// <summary>30-day grace period before permanent deletion.</summary>
    public static readonly TimeSpan GracePeriod = TimeSpan.FromDays(30);

    private readonly UserManager<BlendUser>? _userManager;
    private readonly IRepository<User>? _userRepository;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        ILogger<AccountDeletionService> logger,
        UserManager<BlendUser>? userManager = null,
        IRepository<User>? userRepository = null)
    {
        _logger = logger;
        _userManager = userManager;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<(DateTimeOffset? ScheduledAt, AccountDeletionOpResult Result)> RequestDeletionAsync(
        string userId,
        string? password,
        CancellationToken ct = default)
    {
        if (_userManager is null || _userRepository is null)
        {
            return (null, AccountDeletionOpResult.NotFound);
        }

        var identityUser = await _userManager.FindByIdAsync(userId);
        if (identityUser is null)
        {
            return (null, AccountDeletionOpResult.NotFound);
        }

        // Re-authentication: verify password when the account has a password set.
        var hasPassword = await _userManager.HasPasswordAsync(identityUser);
        if (hasPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (null, AccountDeletionOpResult.ReAuthRequired);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, password);
            if (!passwordValid)
            {
                return (null, AccountDeletionOpResult.ReAuthRequired);
            }
        }

        var cosmosUser = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (cosmosUser is null)
        {
            return (null, AccountDeletionOpResult.NotFound);
        }

        if (cosmosUser.DeletionRequestedAt.HasValue)
        {
            return (null, AccountDeletionOpResult.AlreadyRequested);
        }

        var requestedAt = DateTimeOffset.UtcNow;
        var scheduledAt = requestedAt.Add(GracePeriod);

        await _userRepository.UpdateAsync(
            new User
            {
                Id = cosmosUser.Id,
                Email = cosmosUser.Email,
                DisplayName = cosmosUser.DisplayName,
                ProfilePhotoUrl = cosmosUser.ProfilePhotoUrl,
                PasswordHashRef = cosmosUser.PasswordHashRef,
                Preferences = cosmosUser.Preferences,
                MeasurementUnit = cosmosUser.MeasurementUnit,
                Settings = cosmosUser.Settings,
                CreatedAt = cosmosUser.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                UnreadNotificationCount = cosmosUser.UnreadNotificationCount,
                Role = cosmosUser.Role,
                DeletionRequestedAt = requestedAt,
                IsDeactivated = true,
            },
            userId,
            userId,
            ct);

        _logger.LogInformation("Account deletion requested for user {UserId}. Permanent deletion scheduled at {ScheduledAt}.", userId, scheduledAt);

        return (scheduledAt, AccountDeletionOpResult.Success);
    }

    /// <inheritdoc />
    public async Task<AccountDeletionOpResult> CancelDeletionAsync(
        string userId,
        CancellationToken ct = default)
    {
        if (_userRepository is null)
        {
            return AccountDeletionOpResult.NotFound;
        }

        var cosmosUser = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (cosmosUser is null)
        {
            return AccountDeletionOpResult.NotFound;
        }

        if (!cosmosUser.DeletionRequestedAt.HasValue)
        {
            return AccountDeletionOpResult.NoPendingRequest;
        }

        // Verify still within grace period
        var scheduledDeletion = cosmosUser.DeletionRequestedAt.Value.Add(GracePeriod);
        if (DateTimeOffset.UtcNow >= scheduledDeletion)
        {
            return AccountDeletionOpResult.NoPendingRequest;
        }

        await _userRepository.UpdateAsync(
            new User
            {
                Id = cosmosUser.Id,
                Email = cosmosUser.Email,
                DisplayName = cosmosUser.DisplayName,
                ProfilePhotoUrl = cosmosUser.ProfilePhotoUrl,
                PasswordHashRef = cosmosUser.PasswordHashRef,
                Preferences = cosmosUser.Preferences,
                MeasurementUnit = cosmosUser.MeasurementUnit,
                Settings = cosmosUser.Settings,
                CreatedAt = cosmosUser.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                UnreadNotificationCount = cosmosUser.UnreadNotificationCount,
                Role = cosmosUser.Role,
                DeletionRequestedAt = null,
                IsDeactivated = false,
            },
            userId,
            userId,
            ct);

        _logger.LogInformation("Account deletion cancelled for user {UserId}.", userId);

        return AccountDeletionOpResult.Success;
    }
}
