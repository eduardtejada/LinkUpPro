using LinkUpPro.Core.Application.DTOs.Email;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailRequestDto request);
    Task SendAccountActivationEmailAsync(string email, string userName, string activationUrl);
    Task SendPasswordResetEmailAsync(string email, string userName, string resetUrl);
}
