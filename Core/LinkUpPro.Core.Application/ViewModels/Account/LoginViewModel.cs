using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario o correo electrónico es obligatorio.")]
    [Display(Name = "Usuario o Correo")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada (7 días)")]
    public bool RememberMe { get; set; } = false;
}
