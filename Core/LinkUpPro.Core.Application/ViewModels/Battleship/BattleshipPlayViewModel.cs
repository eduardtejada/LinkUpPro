using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipPlayViewModel
{
    public int GameId { get; set; }
    public GameStatus Status { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;
    public string Player1Id { get; set; } = string.Empty;
    public string Player2Id { get; set; } = string.Empty;

    public string OpponentId { get; set; } = string.Empty;
    public string OpponentFullName { get; set; } = string.Empty;
    public string OpponentUserName { get; set; } = string.Empty;
    public string? OpponentProfilePictureUrl { get; set; }

    public bool IsMyTurn { get; set; }
    public string CurrentTurnUserName { get; set; } = string.Empty;

    public DateTime? LastMoveAt { get; set; }
    public double? RemainingHoursToTimeout { get; set; }

    public string? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public bool? DidIWin { get; set; }
    public GameEndReason EndReason { get; set; }

    public int BoardSize { get; set; } = 12;

    // Tablero propio (12x12) con los barcos del usuario e impactos recibidos
    public List<List<BoardCellViewModel>> MyBoard { get; set; } = new();

    // Tablero de radar/ataque (12x12) con los ataques realizados por el usuario
    public List<List<BoardCellViewModel>> RadarBoard { get; set; } = new();

    // Estado de la flota
    public List<BattleshipShipDetailViewModel> MyShips { get; set; } = new();
    public List<BattleshipShipDetailViewModel> OpponentShips { get; set; } = new();

    public int MyShipsSunkCount => MyShips.Count(s => s.IsSunk);
    public int OpponentShipsSunkCount => OpponentShips.Count(s => s.IsSunk);
    public int TotalShips => 5;

    public string? StatusMessage { get; set; }
}
