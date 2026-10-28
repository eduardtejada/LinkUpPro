using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Infrastructure.Shared.Identity;

public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError
        {
            Code = nameof(DefaultError),
            Description = "Ha ocurrido un error inesperado."
        };
    }

    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = "Conflicto de concurrencia: la información ya ha sido modificada por otro proceso."
        };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = "La contraseña actual es incorrecta."
        };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError
        {
            Code = nameof(InvalidToken),
            Description = "El token de seguridad no es válido o ha expirado."
        };
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = "Ya existe un usuario con esa cuenta de inicio de sesión."
        };
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = $"El nombre de usuario '{userName}' no es válido. Solo puede contener letras y números."
        };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = $"La dirección de correo electrónico '{email}' no tiene un formato válido."
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = $"El nombre de usuario '{userName}' ya está en uso."
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = $"El correo electrónico '{email}' ya está registrado."
        };
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return new IdentityError
        {
            Code = nameof(InvalidRoleName),
            Description = $"El nombre de rol '{role}' no es válido."
        };
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateRoleName),
            Description = $"El nombre de rol '{role}' ya existe."
        };
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = "El usuario ya tiene una contraseña asignada."
        };
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return new IdentityError
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = "El bloqueo de cuenta no está habilitado para este usuario."
        };
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyInRole),
            Description = $"El usuario ya pertenece al rol '{role}'."
        };
    }

    public override IdentityError UserNotInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserNotInRole),
            Description = $"El usuario no pertenece al rol '{role}'."
        };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"La contraseña debe tener al menos {length} caracteres."
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "La contraseña debe contener al menos un carácter especial (!@#$%^&*)."
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "La contraseña debe contener al menos un número (0-9)."
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "La contraseña debe contener al menos una letra minúscula."
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "La contraseña debe contener al menos una letra mayúscula."
        };
    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"La contraseña debe contener al menos {uniqueChars} caracteres diferentes."
        };
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return new IdentityError
        {
            Code = nameof(RecoveryCodeRedemptionFailed),
            Description = "No se pudo canjear el código de recuperación."
        };
    }
}
