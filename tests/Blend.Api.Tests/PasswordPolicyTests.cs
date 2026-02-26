using Blend.Api.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Blend.Api.Tests;

public class PasswordPolicyTests
{
    private readonly PasswordHasher<BlendUser> _hasher = new();

    [Theory]
    [InlineData("short1A", false)]
    [InlineData("alllower1", false)]
    [InlineData("ALLUPPER1", false)]
    [InlineData("NoDigitsHere", false)]
    [InlineData("ValidPass1", true)]
    [InlineData("Str0ngPass", true)]
    public async Task PasswordValidation_EnforcesPolicy(string password, bool expectedValid)
    {
        var userManager = CreateUserManager();
        var user = new BlendUser { UserName = "test@test.com" };
        var result = await userManager.PasswordValidators.First().ValidateAsync(userManager, user, password);
        result.Succeeded.Should().Be(expectedValid);
    }

    [Fact]
    public void PasswordHasher_HashAndVerify_Works()
    {
        var user = new BlendUser();
        var hash = _hasher.HashPassword(user, "ValidPass1");
        var verifyResult = _hasher.VerifyHashedPassword(user, hash, "ValidPass1");
        verifyResult.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void PasswordHasher_WrongPassword_Fails()
    {
        var user = new BlendUser();
        var hash = _hasher.HashPassword(user, "ValidPass1");
        var verifyResult = _hasher.VerifyHashedPassword(user, hash, "WrongPass1");
        verifyResult.Should().Be(PasswordVerificationResult.Failed);
    }

    private static UserManager<BlendUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<BlendUser>>();
        var options = Options.Create(new IdentityOptions
        {
            Password =
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = false,
                RequiredLength = 8
            }
        });
        var passwordHashers = new List<IPasswordHasher<BlendUser>> { new PasswordHasher<BlendUser>() };
        var userValidators = new List<IUserValidator<BlendUser>>();
        var passwordValidators = new List<IPasswordValidator<BlendUser>> { new PasswordValidator<BlendUser>() };
        var logger = new Mock<ILogger<UserManager<BlendUser>>>();
        return new UserManager<BlendUser>(
            store.Object, options, passwordHashers[0],
            userValidators, passwordValidators,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            logger.Object);
    }
}
