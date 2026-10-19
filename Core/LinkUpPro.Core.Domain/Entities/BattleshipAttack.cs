using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities;

public class BattleshipAttack : BaseEntity
{
    public int GameId { get; set; }
    public BattleshipGame Game { get; set; } = null!;

    public string AttackerId { get; set; } = string.Empty;
    public ApplicationUser Attacker { get; set; } = null!;

    // Fila y columna atacada en el tablero 12x12 (0 a 11)
    public int Row { get; set; }
    public int Col { get; set; }

    // true = Acierto (Rojo), false = Fallo (Verde)
    public bool IsHit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
