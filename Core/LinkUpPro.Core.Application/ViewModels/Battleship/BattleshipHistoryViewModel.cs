using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipHistoryViewModel
{
    public int GameId { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;

    public string Player1Name { get; set; } = string.Empty;
    public string Player2Name { get; set; } = string.Empty;

    public string OpponentName { get; set; } = string.Empty;
    public string? WinnerName { get; set; }
    public bool DidIWin { get; set; }
    public GameEndReason EndReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public int BoardSize { get; set; } = 12;

    // Tablero con la colocación de barcos del usuario y sus impactos recibidos
    public List<List<BoardCellViewModel>> MyFleetBoard { get; set; } = new();

    // Tablero de ataques realizados por el usuario
    public List<List<BoardCellViewModel>> MyAttacksBoard { get; set; } = new();

    // Tablero de ataques realizados por el oponente
    public List<List<BoardCellViewModel>> OpponentAttacksBoard { get; set; } = new();
}
