using Blend.Api.Auth.Models;
using Blend.Api.Auth.Services;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Auth.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refresh_token";

    private readonly UserManager<BlendUser> _userManager;
    private readonly SignInManager<BlendUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IEmailService _emailService;

    public AuthController(
        UserManager<BlendUser> userManager,
        SignInManager<BlendUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: "DisplayName, Email, and Password are required.");
        }

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: "Email already registered.");
        }

        var user = new BlendUser
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = request.DisplayName,
            Email = request.Email,
            UserName = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Registration failed",
                detail: errors);
        }

        return await IssueTokensAndRespond(user, StatusCodes.Status201Created, ct);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid email or password.");
        }

        return await IssueTokensAndRespond(user, StatusCodes.Status200OK, ct);
    }

    [HttpPost("login/{provider}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult ExternalLogin([FromRoute] string provider)
    {
        var redirectUri = Url.Action(nameof(ExternalLoginCallback), "Auth", new { provider });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUri);
        return Challenge(properties, provider);
    }

    [HttpGet("callback/{provider}")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalLoginCallback([FromRoute] string provider, CancellationToken ct)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "External login failed",
                detail: "Could not retrieve external login information.");
        }

        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user is not null)
        {
            return await IssueTokensAndRespond(user, StatusCodes.Status200OK, ct);
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? info.Principal.FindFirst("email")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "External login failed",
                detail: "Email claim not provided by the external provider.");
        }

        user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var displayName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                              ?? email;
            user = new BlendUser
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = displayName,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Account creation failed",
                    detail: errors);
            }
        }

        await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
        return await IssueTokensAndRespond(user, StatusCodes.Status200OK, ct);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var cookieToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(cookieToken))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Refresh token not found.");
        }

        var refreshToken = await _refreshTokenService.GetValidAsync(cookieToken, ct);
        if (refreshToken is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Refresh token is invalid or expired.");
        }

        await _refreshTokenService.RevokeAsync(cookieToken, ct);

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "User not found.");
        }

        return await IssueTokensAndRespond(user, StatusCodes.Status200OK, ct);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var cookieToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            await _refreshTokenService.RevokeAsync(cookieToken, ct);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email, token, ct);
        }

        return Ok(new { message = "If that email is registered, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "Invalid token.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Password reset failed",
                detail: errors);
        }

        return Ok();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<IActionResult> IssueTokensAndRespond(BlendUser user, int statusCode, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshToken = await _refreshTokenService.CreateAsync(user.Id, rawRefreshToken, ct);

        Response.Cookies.Append(RefreshTokenCookieName, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt,
        });

        var response = new AuthResponse(accessToken);
        return StatusCode(statusCode, response);
    }
}
