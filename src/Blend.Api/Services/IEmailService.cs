namespace Blend.Api.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
    Task SendEmailConfirmationAsync(string email, string confirmToken, string confirmUrl);
}
