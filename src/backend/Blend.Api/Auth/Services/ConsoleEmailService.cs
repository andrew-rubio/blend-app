namespace Blend.Api.Auth.Services;

public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    // NOTE: This is a development-only implementation. In production, replace with a real email service.
    // The reset token is intentionally logged only at Debug level to prevent accidental exposure in
    // production log aggregators that typically filter below Information.
    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset requested for {Email}.", email);
        _logger.LogDebug("Password reset token for {Email}: {Token}", email, resetToken);
        return Task.CompletedTask;
    }
}
