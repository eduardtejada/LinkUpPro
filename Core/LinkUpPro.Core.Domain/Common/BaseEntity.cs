namespace LinkUpPro.Core.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Centraliza la clave primaria para mantener consistencia.
/// </summary>
/// <typeparam name="TId">Tipo de dato del identificador (ej. int, Guid).</typeparam>
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
}

/// <summary>
/// Atajo para entidades con clave primaria entera.
/// </summary>
public abstract class BaseEntity : BaseEntity<int>
{
}
