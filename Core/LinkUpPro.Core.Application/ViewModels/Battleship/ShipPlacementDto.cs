using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class ShipPlacementDto
{
    public ShipType ShipType { get; set; }
    public int Size { get; set; }
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public ShipDirection Direction { get; set; }
}
