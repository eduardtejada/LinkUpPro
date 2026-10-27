using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipAttackInputModel
{
    [Required]
    public int GameId { get; set; }

    [Range(0, 11, ErrorMessage = "La fila debe estar entre 0 y 11.")]
    public int Row { get; set; }

    [Range(0, 11, ErrorMessage = "La columna debe estar entre 0 y 11.")]
    public int Col { get; set; }
}
