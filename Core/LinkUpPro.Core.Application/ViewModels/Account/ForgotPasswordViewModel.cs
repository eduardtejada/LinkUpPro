using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;
}
