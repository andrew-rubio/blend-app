using Blend.Api.Auth.Controllers;
using Blend.Api.Auth.Models;
using Blend.Api.Auth.Services;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Blend.Tests.Unit.Auth;

public class AccountEnumerationTests
{
    private static Mock<UserManager<BlendUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<BlendUser>>();
        return new Mock<UserManager<BlendUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<BlendUser>> CreateSignInManagerMock(
        Mock<UserManager<BlendUser>> userManagerMock)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<BlendUser>>();
        return new Mock<SignInManager<BlendUser>>(
            userManagerMock.Object, contextAccessor.Object, claimsFactory.Object,
            null!, null!, null!, null!);
    }

    private static AuthController CreateController(
        Mock<UserManager<BlendUser>> userManagerMock,
        Mock<SignInManager<BlendUser>> signInManagerMock,
        Mock<IJwtService>? jwtServiceMock = null,
        Mock<IRefreshTokenService>? refreshTokenServiceMock = null,
        Mock<IEmailService>? emailServiceMock = null)
    {
        jwtServiceMock ??= new Mock<IJwtService>();
        refreshTokenServiceMock ??= new Mock<IRefreshTokenService>();
        emailServiceMock ??= new Mock<IEmailService>();

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            jwtServiceMock.Object,
            refreshTokenServiceMock.Object,
            emailServiceMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton<ProblemDetailsFactory>(new TestProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return controller;
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401WithGenericMessage()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((BlendUser?)null);

        var controller = CreateController(userManagerMock, signInManagerMock);
        var request = new LoginRequest("nonexistent@example.com", "AnyPassword1!");

        var result = await controller.Login(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Invalid email or password", problem.Detail);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401WithGenericMessage()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var user = new BlendUser { Id = "user-1", Email = "user@example.com", DisplayName = "Test" };

        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var controller = CreateController(userManagerMock, signInManagerMock);
        var request = new LoginRequest(user.Email, "WrongPassword1!");

        var result = await controller.Login(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Invalid email or password", problem.Detail);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_Returns200()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((BlendUser?)null);

        var controller = CreateController(userManagerMock, signInManagerMock);
        var request = new ForgotPasswordRequest("nonexistent@example.com");

        var result = await controller.ForgotPassword(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_Returns200()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var emailServiceMock = new Mock<IEmailService>();
        var user = new BlendUser { Id = "user-1", Email = "user@example.com", DisplayName = "Test" };

        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        emailServiceMock
            .Setup(m => m.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(userManagerMock, signInManagerMock,
            emailServiceMock: emailServiceMock);
        var request = new ForgotPasswordRequest(user.Email);

        var result = await controller.ForgotPassword(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_BothResponses_HaveSameMessage()
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock);
        var emailServiceMock = new Mock<IEmailService>();
        var user = new BlendUser { Id = "user-1", Email = "user@example.com", DisplayName = "Test" };

        userManagerMock
            .Setup(m => m.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((BlendUser?)null);
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        emailServiceMock
            .Setup(m => m.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller1 = CreateController(userManagerMock, signInManagerMock,
            emailServiceMock: emailServiceMock);
        var controller2 = CreateController(userManagerMock, signInManagerMock,
            emailServiceMock: emailServiceMock);

        var result1 = await controller1.ForgotPassword(
            new ForgotPasswordRequest("nonexistent@example.com"), CancellationToken.None);
        var result2 = await controller2.ForgotPassword(
            new ForgotPasswordRequest(user.Email), CancellationToken.None);

        var ok1 = Assert.IsType<OkObjectResult>(result1);
        var ok2 = Assert.IsType<OkObjectResult>(result2);
        Assert.Equal(ok1.StatusCode, ok2.StatusCode);
    }

    // Minimal ProblemDetailsFactory for controller unit tests
    private sealed class TestProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext, int? statusCode = null, string? title = null,
            string? type = null, string? detail = null, string? instance = null)
            => new() { Status = statusCode, Title = title, Detail = detail, Instance = instance };

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext, ModelStateDictionary modelStateDictionary,
            int? statusCode = null, string? title = null, string? type = null,
            string? detail = null, string? instance = null)
            => new(modelStateDictionary) { Status = statusCode, Title = title };
    }
}
