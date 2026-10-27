namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipAttackResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public string? SunkShipName { get; set; }

    public bool IsGameOver { get; set; }
    public string? WinnerName { get; set; }
}
