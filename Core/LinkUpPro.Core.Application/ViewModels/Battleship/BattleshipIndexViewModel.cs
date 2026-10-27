using LinkUpPro.Core.Application.ViewModels.Friend;

namespace LinkUpPro.Core.Application.ViewModels.Battleship;

public class BattleshipIndexViewModel
{
    public List<BattleshipGameItemViewModel> ActiveGames { get; set; } = new();
    public List<BattleshipGameItemViewModel> FinishedGames { get; set; } = new();
    public List<FriendViewModel> AvailableFriends { get; set; } = new();
}
