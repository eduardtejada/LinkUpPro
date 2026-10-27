using System.Security.Claims;
using LinkUpPro.Core.Application.DTOs.Account;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Account;
using LinkUpPro.Core.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LinkUpPro.Infrastructure.Shared.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        IMemoryCache memoryCache,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<AccountResponseDto> RegisterAsync(
        RegisterViewModel vm, 
        Stream? profilePictureStream, 
        string? profilePictureFileName, 
        string originUrl)
    {
        var response = new AccountResponseDto();

        var existingUserByName = await _userManager.FindByNameAsync(vm.UserName);
        if (existingUserByName != null)
        {
            response.HasError = true;
            response.Error = $"El nombre de usuario '{vm.UserName}' ya está en uso.";
            return response;
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
        if (existingUserByEmail != null)
        {
            response.HasError = true;
            response.Error = $"El correo electrónico '{vm.Email}' ya está registrado.";
            return response;
        }

        string? profilePictureUrl = null;
        if (profilePictureStream != null && !string.IsNullOrWhiteSpace(profilePictureFileName))
        {
            if (!_fileStorageService.ValidateImage(profilePictureFileName, profilePictureStream.Length, out var error))
            {
                response.HasError = true;
                response.Error = error;
                return response;
            }

            profilePictureUrl = await _fileStorageService.SaveFileAsync(profilePictureStream, profilePictureFileName, "profiles");
        }

        var user = new ApplicationUser
        {
            UserName = vm.UserName.Trim(),
            Email = vm.Email.Trim().ToLowerInvariant(),
            FirstName = vm.FirstName.Trim(),
            LastName = vm.LastName.Trim(),
            PhoneNumber = vm.PhoneNumber.Trim(),
            ProfilePictureUrl = profilePictureUrl,
            IsActive = false,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, vm.Password);
        if (!createResult.Succeeded)
        {
            response.HasError = true;
            response.Errors = createResult.Errors.Select(e => e.Description).ToList();
            response.Error = string.Join(" ", response.Errors);

            if (profilePictureUrl != null)
            {
                _fileStorageService.DeleteFile(profilePictureUrl);
            }

            return response;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var activationUrl = $"{originUrl}/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendAccountActivationEmailAsync(user.Email, user.UserName, activationUrl);

        response.SuccessMessage = "Registro completado con éxito. Por favor revisa tu correo electrónico para activar tu cuenta antes de iniciar sesión.";
        return response;
    }

    public async Task<AccountResponseDto> ConfirmAccountAsync(string userId, string token)
    {
        var response = new AccountResponseDto();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            response.HasError = true;
            response.Error = "Parámetros de activación no válidos.";
            return response;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            response.HasError = true;
            response.Error = "Usuario no encontrado.";
            return response;
        }

        if (user.IsActive && user.EmailConfirmed)
        {
            response.SuccessMessage = "Tu cuenta ya había sido activada previamente. Puedes iniciar sesión.";
            return response;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            response.HasError = true;
            response.Error = "El enlace de activación es inválido o ha expirado. Puedes solicitar un nuevo correo de activación.";
            return response;
        }

        // Activar cuenta en el modelo de dominio
        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        response.SuccessMessage = "¡Tu cuenta ha sido activada exitosamente! Ya puedes iniciar sesión.";
        return response;
    }

    public async Task<AccountResponseDto> ResendActivationEmailAsync(string email, string originUrl)
    {
        var response = new AccountResponseDto();

        if (string.IsNullOrWhiteSpace(email))
        {
            response.HasError = true;
            response.Error = "Debe proporcionar una dirección de correo válida.";
            return response;
        }

        var user = await _userManager.FindByEmailAsync(email);

        // Mensaje genérico para no revelar si la cuenta existe o no
        const string genericMessage = "Si la dirección de correo ingresada está registrada y requiere activación, hemos enviado un nuevo enlace.";

        if (user == null || user.IsActive)
        {
            response.SuccessMessage = genericMessage;
            return response;
        }

        // Control de espera mínima de 5 minutos
        var cooldownKey = $"ResendActivationCooldown_{user.Id}";
        if (_memoryCache.TryGetValue(cooldownKey, out _))
        {
            response.HasError = true;
            response.Error = "Debes esperar al menos 5 minutos entre solicitudes de reenvío de correo.";
            return response;
        }

        // Guardar cooldown de 5 minutos en caché
        _memoryCache.Set(cooldownKey, true, TimeSpan.FromMinutes(5));

        // Generar token y enviar correo
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var activationUrl = $"{originUrl}/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendAccountActivationEmailAsync(user.Email!, user.UserName!, activationUrl);

        response.SuccessMessage = genericMessage;
        return response;
    }

    public async Task<AccountResponseDto> LoginAsync(LoginViewModel vm)
    {
        var response = new AccountResponseDto();

        // Buscar por usuario o correo (insensible a mayúsculas)
        var user = await _userManager.FindByNameAsync(vm.UserNameOrEmail) 
                   ?? await _userManager.FindByEmailAsync(vm.UserNameOrEmail);

        if (user == null)
        {
            response.HasError = true;
            response.Error = "Usuario, correo o contraseña incorrectos.";
            return response;
        }

        // Verificar si la cuenta está bloqueada temporalmente
        if (await _userManager.IsLockedOutAsync(user))
        {
            response.HasError = true;
            response.IsLockedOut = true;
            response.Error = "Tu cuenta está bloqueada temporalmente por 15 minutos debido a 5 intentos fallidos consecutivos.";
            return response;
        }

        // Validar contraseña y registrar intento fallido si no coincide
        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, vm.Password, lockoutOnFailure: true);

        if (passwordCheck.IsLockedOut)
        {
            response.HasError = true;
            response.IsLockedOut = true;
            response.Error = "Has excedido el número de intentos permitidos (5). Tu cuenta ha sido bloqueada temporalmente por 15 minutos.";
            return response;
        }

        if (!passwordCheck.Succeeded)
        {
            response.HasError = true;
            response.Error = "Usuario, correo o contraseña incorrectos.";
            return response;
        }

        // Verificar si la cuenta está inactiva
        if (!user.IsActive || !user.EmailConfirmed)
        {
            response.HasError = true;
            response.IsAccountInactive = true;
            response.Error = "Tu cuenta está inactiva. Debes activarla mediante el enlace enviado a tu correo antes de iniciar sesión.";
            return response;
        }

        // Iniciar sesión con cookies persistentes (7 días) o por sesión (30 minutos)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = vm.RememberMe,
            ExpiresUtc = vm.RememberMe 
                ? DateTimeOffset.UtcNow.AddDays(7) 
                : DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await _signInManager.SignInWithClaimsAsync(user, authProperties, new List<Claim>());

        response.SuccessMessage = "Inicio de sesión exitoso.";
        return response;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<AccountResponseDto> ForgotPasswordAsync(ForgotPasswordViewModel vm, string originUrl)
    {
        var response = new AccountResponseDto();

        // Mensaje genérico para no revelar si la cuenta existe
        const string genericMessage = "Si la dirección de correo ingresada está registrada, hemos enviado las instrucciones para restablecer tu contraseña.";

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user == null || !user.IsActive)
        {
            response.SuccessMessage = genericMessage;
            return response;
        }

        // Generar token para restablecer contraseña (válido por 1 hora)
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = $"{originUrl}/Account/ResetPassword?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.UserName!, resetUrl);

        response.SuccessMessage = genericMessage;
        return response;
    }

    public async Task<AccountResponseDto> ResetPasswordAsync(ResetPasswordViewModel vm)
    {
        var response = new AccountResponseDto();

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            response.HasError = true;
            response.Error = "No se pudo procesar la solicitud.";
            return response;
        }

        var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
        if (!result.Succeeded)
        {
            response.HasError = true;
            response.Errors = result.Errors.Select(e => e.Description).ToList();
            response.Error = string.Join(" ", response.Errors);
            return response;
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        response.SuccessMessage = "Tu contraseña ha sido restablecida con éxito. Ya puedes iniciar sesión con tu nueva contraseña.";
        return response;
    }

    public async Task<ProfileViewModel?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new ProfileViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CurrentProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    public async Task<AccountResponseDto> UpdateProfileAsync(
        string userId, 
        ProfileViewModel vm, 
        Stream? profilePictureStream, 
        string? profilePictureFileName)
    {
        var response = new AccountResponseDto();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            response.HasError = true;
            response.Error = "Usuario no encontrado.";
            return response;
        }

        // Actualizar datos editables
        user.FirstName = vm.FirstName.Trim();
        user.LastName = vm.LastName.Trim();
        user.PhoneNumber = vm.PhoneNumber.Trim();

        // Procesar nueva foto si se proporcionó
        if (profilePictureStream != null && !string.IsNullOrWhiteSpace(profilePictureFileName))
        {
            if (!_fileStorageService.ValidateImage(profilePictureFileName, profilePictureStream.Length, out var error))
            {
                response.HasError = true;
                response.Error = error;
                return response;
            }

            var oldPicture = user.ProfilePictureUrl;
            var newPictureUrl = await _fileStorageService.SaveFileAsync(profilePictureStream, profilePictureFileName, "profiles");
            user.ProfilePictureUrl = newPictureUrl;

            // Eliminar imagen anterior del disco
            if (!string.IsNullOrWhiteSpace(oldPicture))
            {
                _fileStorageService.DeleteFile(oldPicture);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            response.HasError = true;
            response.Errors = updateResult.Errors.Select(e => e.Description).ToList();
            response.Error = string.Join(" ", response.Errors);
            return response;
        }

        response.SuccessMessage = "Perfil actualizado exitosamente.";
        return response;
    }

    public async Task<AccountResponseDto> ChangePasswordAsync(string userId, ChangePasswordViewModel vm)
    {
        var response = new AccountResponseDto();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            response.HasError = true;
            response.Error = "Usuario no encontrado.";
            return response;
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        if (!changeResult.Succeeded)
        {
            response.HasError = true;
            response.Errors = changeResult.Errors.Select(e => e.Description).ToList();
            response.Error = string.Join(" ", response.Errors);
            return response;
        }

        // Invalidar sesiones previas y refrescar la sesión actual
        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        response.SuccessMessage = "Contraseña cambiada exitosamente.";
        return response;
    }
}
