using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LinkUpPro.Core.Application.Validation;

/// <summary>
/// Valida que el número de teléfono corresponda a un formato dominicano válido:
/// 809-xxx-xxxx, 829-xxx-xxxx o 849-xxx-xxxx.
/// </summary>
public class DominicanPhoneAttribute : ValidationAttribute
{
    private static readonly Regex DominicanPhoneRegex = 
        new(@"^8(09|29|49)[- ]?[0-9]{3}[- ]?[0-9]{4}$", RegexOptions.Compiled);

    public DominicanPhoneAttribute() 
        : base("El número de teléfono debe tener un formato dominicano válido: 809-xxx-xxxx, 829-xxx-xxxx u 849-xxx-xxxx.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            // Si el campo es obligatorio, debe usarse [Required] junto con este atributo
            return ValidationResult.Success;
        }

        var phone = value.ToString()!.Trim();

        if (!DominicanPhoneRegex.IsMatch(phone))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
