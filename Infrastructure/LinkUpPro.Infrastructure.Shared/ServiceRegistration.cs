using LinkUpPro.Core.Application.DTOs.Settings;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Enlazar configuraciones tipadas de correo desde appsettings.json
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Registrar caché en memoria para el control de reenvíos (cooldown de 5 min)
        services.AddMemoryCache();

        // Registrar servicios compartidos
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileStorageService, FileStorageService>();
        services.AddTransient<IAccountService, AccountService>();

        return services;
    }
}
