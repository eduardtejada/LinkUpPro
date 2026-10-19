using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [Display(Name = "Nueva Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar Nueva Contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
