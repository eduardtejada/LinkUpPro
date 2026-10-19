using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña Actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [Display(Name = "Nueva Contraseña")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar Nueva Contraseña")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
