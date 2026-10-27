using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipShipDetailViewModel
{
    public ShipType ShipType { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool IsSunk { get; set; }
    public int HitsCount { get; set; }
}
