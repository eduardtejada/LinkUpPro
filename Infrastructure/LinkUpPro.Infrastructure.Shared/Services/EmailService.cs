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
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost) || 
                _emailSettings.SmtpHost.Contains("example.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("SMTP no configurado en appsettings.json. Se omite conexión remota.");
                return;
            }

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
            smtp.Timeout = 3000; // Timeout de máximo 3 segundos

            await smtp.ConnectAsync(
                _emailSettings.SmtpHost, 
                _emailSettings.SmtpPort, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUser) && !string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
            {
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado exitosamente a {To}", request.To);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar al servidor SMTP para {To}. La ejecución continúa con normalidad.", request.To);
        }
    }

    public async Task SendAccountActivationEmailAsync(string email, string userName, string activationUrl)
    {
        Console.WriteLine("\n==================================================================");
        Console.WriteLine($"[Copia de Facebook] ENLACE DE ACTIVACIÓN PARA: {email}");
        Console.WriteLine(activationUrl);
        Console.WriteLine("==================================================================\n");

        var subject = "Activa tu cuenta en Copia de Facebook";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #1877f2; text-align: center;">¡Bienvenido a Copia de Facebook, {userName}!</h2>
                <p>Gracias por unirte a nuestra comunidad. Para activar tu cuenta, haz clic en el siguiente botón:</p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{activationUrl}" style="background-color: #1877f2; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">Activar mi cuenta</a>
                </div>
                <p style="color: #65676b; font-size: 0.9em;">Este enlace es válido por 24 horas y de un solo uso.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="font-size: 0.8em; color: #8a8d91; text-align: center;">Copia de Facebook &copy; 2026</p>
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
        Console.WriteLine("\n==================================================================");
        Console.WriteLine($"[Copia de Facebook] ENLACE DE RECUPERACIÓN PARA: {email}");
        Console.WriteLine(resetUrl);
        Console.WriteLine("==================================================================\n");

        var subject = "Restablece tu contraseña - Copia de Facebook";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #1877f2; text-align: center;">Recuperación de Contraseña</h2>
                <p>Hola {userName}, recibimos una solicitud para restablecer tu contraseña. Haz clic a continuación:</p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{resetUrl}" style="background-color: #1877f2; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">Restablecer mi contraseña</a>
                </div>
                <p style="color: #65676b; font-size: 0.9em;">Este enlace es válido por 1 hora.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="font-size: 0.8em; color: #8a8d91; text-align: center;">Copia de Facebook &copy; 2026</p>
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
