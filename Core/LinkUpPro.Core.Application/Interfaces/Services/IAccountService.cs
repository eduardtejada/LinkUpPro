using LinkUpPro.Core.Application.DTOs.Account;
using LinkUpPro.Core.Application.ViewModels.Account;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IAccountService
{
    Task<AccountResponseDto> RegisterAsync(
        RegisterViewModel vm, 
        Stream? profilePictureStream, 
        string? profilePictureFileName, 
        string originUrl);

    Task<AccountResponseDto> ConfirmAccountAsync(string userId, string token);

    Task<AccountResponseDto> ResendActivationEmailAsync(string email, string originUrl);

    Task<AccountResponseDto> LoginAsync(LoginViewModel vm);

    Task LogoutAsync();

    Task<AccountResponseDto> ForgotPasswordAsync(ForgotPasswordViewModel vm, string originUrl);

    Task<AccountResponseDto> ResetPasswordAsync(ResetPasswordViewModel vm);

    Task<ProfileViewModel?> GetProfileAsync(string userId);

    Task<AccountResponseDto> UpdateProfileAsync(
        string userId, 
        ProfileViewModel vm, 
        Stream? profilePictureStream, 
        string? profilePictureFileName);

    Task<AccountResponseDto> ChangePasswordAsync(string userId, ChangePasswordViewModel vm);
}
