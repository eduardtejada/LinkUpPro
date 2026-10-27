using LinkUpPro.Core.Application.ViewModels.Battleship;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IBattleshipService
{
    Task<BattleshipIndexViewModel> GetIndexViewModelAsync(string currentUserId);

    Task<(bool Success, string? ErrorMessage, int? GameId)> CreateGameChallengeAsync(string currentUserId, string opponentId);

    Task<(bool Success, string? ErrorMessage, BattleshipPlacementViewModel? ViewModel)> GetPlacementViewModelAsync(int gameId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> SavePlacementAsync(string currentUserId, int gameId, List<ShipPlacementDto> placements);

    Task<(bool Success, string? ErrorMessage, BattleshipPlayViewModel? ViewModel)> GetPlayViewModelAsync(int gameId, string currentUserId);

    Task<BattleshipAttackResultDto> ExecuteAttackAsync(string currentUserId, int gameId, int row, int col);

    Task<(bool Success, string? ErrorMessage)> SurrenderGameAsync(string currentUserId, int gameId);

    Task<(bool Success, string? ErrorMessage, BattleshipHistoryViewModel? ViewModel)> GetHistoryViewModelAsync(int gameId, string currentUserId);
}
