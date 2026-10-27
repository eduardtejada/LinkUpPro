using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipGameItemViewModel
{
    public int GameId { get; set; }

    public string OpponentId { get; set; } = string.Empty;
    public string OpponentFullName { get; set; } = string.Empty;
    public string OpponentUserName { get; set; } = string.Empty;
    public string? OpponentProfilePictureUrl { get; set; }

    public GameStatus Status { get; set; }
    public bool IsMyTurn { get; set; }
    public bool IsPlayer1 { get; set; }

    public bool Player1Ready { get; set; }
    public bool Player2Ready { get; set; }
    public bool IsReady { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastMoveAt { get; set; }

    public string? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public bool? DidIWin { get; set; }
    public GameEndReason EndReason { get; set; }
    public DateTime? FinishedAt { get; set; }
}
