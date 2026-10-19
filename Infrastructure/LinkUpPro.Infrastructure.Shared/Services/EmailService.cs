using LinkUpPro.Core.Application.DTOs.Email;
using LinkUpPro.Core.Application.DTOs.Settings;
using LinkUpPro.Core.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinkUpPro.Infrastructure.Shared.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailRequestDto request)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail);
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;

            var builder = new BodyBuilder();
            if (request.IsHtml)
            {
                builder.HtmlBody = request.Body;
            }
            else
            {
                builder.TextBody = request.Body;
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // Conectar al servidor SMTP
            await smtp.ConnectAsync(
                _emailSettings.SmtpHost, 
                _emailSettings.SmtpPort, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            // Autenticar si hay credenciales configuradas
            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUser) && !string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
            {
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado exitosamente a {To} con asunto: {Subject}", request.To, request.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {To}", request.To);
            // No bloqueamos el flujo principal si el servidor SMTP de prueba no responde
        }
    }

    public async Task SendAccountActivationEmailAsync(string email, string userName, string activationUrl)
    {
        var subject = "Activa tu cuenta en LinkUp Pro";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #0d6efd; text-align: center;">¡Bienvenido a LinkUp Pro, {userName}!</h2>
                <p>Gracias por unirte a nuestra comunidad. Para comenzar a interactuar y jugar, necesitas activar tu cuenta haciendo clic en el siguiente enlace:</p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{activationUrl}" style="background-color: #0d6efd; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;">Activar mi cuenta</a>
                </div>
                <p style="color: #6c757d; font-size: 0.9em;">Este enlace es válido por 24 horas y de un solo uso. Si no creaste una cuenta, puedes ignorar este mensaje.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="font-size: 0.8em; color: #adb5bd; text-align: center;">LinkUp Pro &copy; 2026 - Red Social y Batalla Naval</p>
            </div>
            """;

        await SendAsync(new EmailRequestDto
        {
            To = email,
            Subject = subject,
            Body = body,
            IsHtml = true
        });
    }

    public async Task SendPasswordResetEmailAsync(string email, string userName, string resetUrl)
    {
        var subject = "Restablece tu contraseña - LinkUp Pro";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #dc3545; text-align: center;">Recuperación de Contraseña</h2>
                <p>Hola {userName}, hemos recibido una solicitud para restablecer la contraseña de tu cuenta en LinkUp Pro.</p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{resetUrl}" style="background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;">Restablecer Contraseña</a>
                </div>
                <p style="color: #6c757d; font-size: 0.9em;">Este enlace es válido por 1 hora. Si tú no realizaste esta solicitud, puedes ignorar este correo de forma segura.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="font-size: 0.8em; color: #adb5bd; text-align: center;">LinkUp Pro &copy; 2026 - Seguridad de Cuentas</p>
            </div>
            """;

        await SendAsync(new EmailRequestDto
        {
            To = email,
            Subject = subject,
            Body = body,
            IsHtml = true
        });
    }
}
