using System.Text.RegularExpressions;
using Blend.Api.Profile.Models;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Profile.Services;

public sealed class ProfileService : IProfileService
{
    private static readonly Regex DisplayNamePattern = new(@"^[a-zA-Z0-9 _\-\.]+$", RegexOptions.Compiled);
    private const int DisplayNameMinLength = 2;
    private const int DisplayNameMaxLength = 50;
    private const int BioMaxLength = 500;

    private readonly UserManager<BlendUser> _userManager;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<BlendUser> userManager,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public IReadOnlyList<string> ValidateUpdateProfile(UpdateProfileRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add("Display name is required.");
        }
        else if (request.DisplayName.Length < DisplayNameMinLength)
        {
            errors.Add($"Display name must be at least {DisplayNameMinLength} characters.");
        }
        else if (request.DisplayName.Length > DisplayNameMaxLength)
        {
            errors.Add($"Display name must be {DisplayNameMaxLength} characters or fewer.");
        }
        else if (!DisplayNamePattern.IsMatch(request.DisplayName))
        {
            errors.Add("Display name may only contain letters, numbers, spaces, underscores, hyphens, and periods.");
        }

        if (request.Bio is not null && request.Bio.Length > BioMaxLength)
        {
            errors.Add($"Bio must be {BioMaxLength} characters or fewer.");
        }

        return errors;
    }

    public async Task<MyProfileResponse?> GetMyProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when fetching own profile.", userId);
            return null;
        }

        return new MyProfileResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            JoinDate = user.CreatedAt,
            RecipeCount = user.RecipeCount,
            LikeCount = user.LikeCount,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount,
        };
    }

    public async Task<(MyProfileResponse? Profile, ProfileOpResult Result, IReadOnlyList<string>? Errors)> UpdateMyProfileAsync(
        string userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var errors = ValidateUpdateProfile(request);
        if (errors.Count > 0)
        {
            return (null, ProfileOpResult.ValidationFailed, errors);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when updating profile.", userId);
            return (null, ProfileOpResult.NotFound, null);
        }

        user.DisplayName = request.DisplayName;
        user.Bio = request.Bio;
        user.ProfilePhotoUrl = request.AvatarUrl ?? user.ProfilePhotoUrl;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userManager.UpdateAsync(user);

        return (new MyProfileResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            JoinDate = user.CreatedAt,
            RecipeCount = user.RecipeCount,
            LikeCount = user.LikeCount,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount,
        }, ProfileOpResult.Success, null);
    }

    public async Task<PublicProfileResponse?> GetPublicProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new PublicProfileResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.ProfilePhotoUrl,
            Bio = user.Bio,
            JoinDate = user.CreatedAt,
            RecipeCount = user.RecipeCount,
        };
    }
}
