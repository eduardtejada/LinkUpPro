using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Validation;

namespace LinkUpPro.Core.Application.ViewModels.Account;

public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    // Campos de solo lectura
    [Display(Name = "Nombre de Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    // Campos editables
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
    [Display(Name = "Teléfono")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Foto de Perfil")]
    public string? CurrentProfilePictureUrl { get; set; }
}
