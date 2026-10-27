using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BoardCellViewModel
{
    public int Row { get; set; }
    public int Col { get; set; }

    // Indica si la celda contiene un segmento de barco
    public bool HasShip { get; set; }
    public ShipType? ShipType { get; set; }

    // Disparos
    public bool IsAttacked { get; set; }
    public bool IsHit { get; set; }
    public bool IsMiss => IsAttacked && !IsHit;
}
