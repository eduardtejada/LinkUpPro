using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Validation;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
    [DominicanPhone]
    [Display(Name = "Teléfono Dominicano")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Debe ingresar un correo electrónico válido con dominio, ej. usuario@correo.com")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 30 caracteres.")]
    [Display(Name = "Nombre de Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar Contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
