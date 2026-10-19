using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class BattleshipShip : BaseEntity
{
    public int GameId { get; set; }
    public BattleshipGame Game { get; set; } = null!;

    public string PlayerId { get; set; } = string.Empty;
    public ApplicationUser Player { get; set; } = null!;

    public ShipType ShipType { get; set; }
    public int Size { get; set; }

    // Posición inicial y orientación
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public ShipDirection Direction { get; set; }

    public bool IsSunk { get; set; } = false;

    // Coordenadas individuales ocupadas por este barco
    public ICollection<BattleshipShipCoordinate> Coordinates { get; set; } = new List<BattleshipShipCoordinate>();
}
