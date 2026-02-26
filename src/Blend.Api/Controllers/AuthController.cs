using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Blend.Api.Configuration;
using Blend.Api.Domain;
using Blend.Api.Models.Auth;
using Blend.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blend.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<BlendUser> _userManager;
    private readonly SignInManager<BlendUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private const string RefreshTokenCookieName = "blend_refresh_token";

    public AuthController(
        UserManager<BlendUser> userManager,
        SignInManager<BlendUser> signInManager,
        ITokenService tokenService,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new BlendUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(
                result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())));
        }

        await _userManager.AddToRoleAsync(user, "User");
        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseWithCookie(user, roles);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        const string genericError = "Invalid email or password.";
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = genericError, Status = 401 });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = genericError, Status = 401 });

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseWithCookie(user, roles);
    }

    [HttpPost("login/{provider}")]
    public IActionResult ExternalLogin([FromRoute] string provider)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { provider });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("callback/{provider}")]
    public async Task<IActionResult> ExternalLoginCallback([FromRoute] string provider)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "External login info not available.", Status = 400 });

        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user == null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new BlendUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    DisplayName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Could not create user.", Status = 400 });
                await _userManager.AddToRoleAsync(user, "User");
            }
            await _userManager.AddLoginAsync(user, info);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseWithCookie(user, roles);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "No refresh token.", Status = 401 });

        var tokenHash = HashToken(refreshToken);

        string? userId = null;
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(refreshToken));
            var colonIdx = decoded.IndexOf(':');
            if (colonIdx > 0) userId = decoded[..colonIdx];
        }
        catch { }

        BlendUser? user = null;
        if (userId != null)
            user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive || user.RefreshTokenHash == null)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid refresh token.", Status = 401 });

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Refresh token expired.", Status = 401 });

        if (user.RefreshTokenHash != tokenHash)
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid refresh token.", Status = 401 });

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseWithCookie(user, roles);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            string? userId = null;
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(refreshToken));
                var colonIdx = decoded.IndexOf(':');
                if (colonIdx > 0) userId = decoded[..colonIdx];
            }
            catch { }

            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshTokenHash = null;
                    user.RefreshTokenExpiresAt = null;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = _jwtSettings.PasswordResetBaseUrl;
            await _emailService.SendPasswordResetEmailAsync(request.Email, token, resetUrl);
        }
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid token or email.", Status = 400 });

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invalid token or email.", Status = 400 });

        return Ok(new { message = "Password reset successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (userId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = roles.ToList()
        });
    }

    private async Task<IActionResult> BuildAuthResponseWithCookie(BlendUser user, IList<string> roles)
    {
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var (refreshToken, expiresAt) = _tokenService.GenerateRefreshToken();

        var tokenWithUserId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{refreshToken}"));
        var tokenHash = HashToken(tokenWithUserId);

        user.RefreshTokenHash = tokenHash;
        user.RefreshTokenExpiresAt = expiresAt;
        await _userManager.UpdateAsync(user);

        Response.Cookies.Append(RefreshTokenCookieName, tokenWithUserId, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/auth",
            MaxAge = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
        });

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            TokenType = "Bearer",
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles.ToList()
            }
        });
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
