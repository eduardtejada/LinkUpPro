using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class BattleshipGame : BaseEntity
{
    // Creador de la partida (Jugador 1 - Siempre ataca primero en Fase 2)
    public string Player1Id { get; set; } = string.Empty;
    public ApplicationUser Player1 { get; set; } = null!;

    // Oponente (Jugador 2)
    public string Player2Id { get; set; } = string.Empty;
    public ApplicationUser Player2 { get; set; } = null!;

    public bool Player1Ready { get; set; } = false;
    public bool Player2Ready { get; set; } = false;

    public GameStatus Status { get; set; } = GameStatus.Placement;

    // Control de turnos
    public string? CurrentTurnPlayerId { get; set; }
    public ApplicationUser? CurrentTurnPlayer { get; set; }

    // Resultado final
    public string? WinnerPlayerId { get; set; }
    public ApplicationUser? WinnerPlayer { get; set; }
    public GameEndReason EndReason { get; set; } = GameEndReason.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    
    // Utilizado para verificar el abandono por inactividad tras 48 horas sin ataque
    public DateTime? LastMoveAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Colecciones de la partida
    public ICollection<BattleshipShip> Ships { get; set; } = new List<BattleshipShip>();
    public ICollection<BattleshipAttack> Attacks { get; set; } = new List<BattleshipAttack>();
}
