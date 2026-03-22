namespace Blend.Api.Auth.Services;

public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Password reset requested for {Email}. Reset token: {Token}",
            email,
            resetToken);
        return Task.CompletedTask;
    }
}
