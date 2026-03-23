using Blend.Api.Profile.Models;

namespace Blend.Api.Profile.Services;

public enum ProfileOpResult
{
    Success,
    NotFound,
    ValidationFailed,
}

public interface IProfileService
{
    IReadOnlyList<string> ValidateUpdateProfile(UpdateProfileRequest request);

    Task<MyProfileResponse?> GetMyProfileAsync(string userId, CancellationToken ct = default);

    Task<(MyProfileResponse? Profile, ProfileOpResult Result, IReadOnlyList<string>? Errors)> UpdateMyProfileAsync(
        string userId, UpdateProfileRequest request, CancellationToken ct = default);

    Task<PublicProfileResponse?> GetPublicProfileAsync(string userId, CancellationToken ct = default);
}
