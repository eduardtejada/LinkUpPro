using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class SavePlacementInputModel
{
    [Required]
    public int GameId { get; set; }

    [Required]
    public string ShipsJson { get; set; } = string.Empty;
}
