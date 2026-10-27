using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipPlacementViewModel
{
    public int GameId { get; set; }

    public string OpponentFullName { get; set; } = string.Empty;
    public string OpponentUserName { get; set; } = string.Empty;
    public string? OpponentProfilePictureUrl { get; set; }

    public bool IsReady { get; set; }
    public bool OpponentReady { get; set; }

    public int BoardSize { get; set; } = 12;

    public List<ShipPlacementDto> PlacedShips { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
