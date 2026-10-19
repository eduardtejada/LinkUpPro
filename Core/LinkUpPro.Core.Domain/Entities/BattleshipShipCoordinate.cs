using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities;

public class BattleshipShipCoordinate : BaseEntity
{
    public int ShipId { get; set; }
    public BattleshipShip Ship { get; set; } = null!;

    // Fila y columna dentro del tablero 12x12 (0 a 11)
    public int Row { get; set; }
    public int Col { get; set; }

    // Registra si esta coordenada específica ya recibió un impacto de bala
    public bool IsHit { get; set; } = false;
}
