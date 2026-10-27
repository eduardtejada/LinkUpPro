using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.Repositories;

public interface IBattleshipRepository : IGenericRepository<BattleshipGame>
{
    Task<BattleshipGame?> GetGameWithDetailsAsync(int gameId, bool disableTracking = false);

    Task<IReadOnlyList<BattleshipGame>> GetActiveGamesByUserAsync(string userId);

    Task<IReadOnlyList<BattleshipGame>> GetFinishedGamesByUserAsync(string userId);

    Task<BattleshipGame?> GetActiveGameBetweenUsersAsync(string userAId, string userBId);

    Task<bool> HasActiveGameBetweenUsersAsync(string userAId, string userBId);

    Task AddShipsAsync(IEnumerable<BattleshipShip> ships);

    Task AddAttackAsync(BattleshipAttack attack);
}
