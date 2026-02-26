namespace Blend.Api.Services;

public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger) => _logger = logger;

    public Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        _logger.LogInformation("[DEV EMAIL] Password reset for {Email}: {Url}?token={Token}", email, resetUrl, resetToken);
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string email, string confirmToken, string confirmUrl)
    {
        _logger.LogInformation("[DEV EMAIL] Email confirmation for {Email}: {Url}?token={Token}", email, confirmUrl, confirmToken);
        return Task.CompletedTask;
    }
}
