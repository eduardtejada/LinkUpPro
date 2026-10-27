using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public class BattleshipRepository : GenericRepository<BattleshipGame>, IBattleshipRepository
{
    public BattleshipRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<BattleshipGame?> GetGameWithDetailsAsync(int gameId, bool disableTracking = false)
    {
        var query = _dbContext.BattleshipGames.AsQueryable();

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.CurrentTurnPlayer)
            .Include(g => g.WinnerPlayer)
            .Include(g => g.Ships)
                .ThenInclude(s => s.Coordinates)
            .Include(g => g.Attacks)
                .ThenInclude(a => a.Attacker)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<IReadOnlyList<BattleshipGame>> GetActiveGamesByUserAsync(string userId)
    {
        return await _dbContext.BattleshipGames
            .AsNoTracking()
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.CurrentTurnPlayer)
            .Where(g => (g.Player1Id == userId || g.Player2Id == userId) && g.Status != GameStatus.Finished)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<BattleshipGame>> GetFinishedGamesByUserAsync(string userId)
    {
        return await _dbContext.BattleshipGames
            .AsNoTracking()
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.WinnerPlayer)
            .Where(g => (g.Player1Id == userId || g.Player2Id == userId) && g.Status == GameStatus.Finished)
            .OrderByDescending(g => g.FinishedAt ?? g.CreatedAt)
            .ToListAsync();
    }

    public async Task<BattleshipGame?> GetActiveGameBetweenUsersAsync(string userAId, string userBId)
    {
        return await _dbContext.BattleshipGames
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.Status != GameStatus.Finished &&
                                      ((g.Player1Id == userAId && g.Player2Id == userBId) ||
                                       (g.Player1Id == userBId && g.Player2Id == userAId)));
    }

    public async Task<bool> HasActiveGameBetweenUsersAsync(string userAId, string userBId)
    {
        return await _dbContext.BattleshipGames
            .AnyAsync(g => g.Status != GameStatus.Finished &&
                           ((g.Player1Id == userAId && g.Player2Id == userBId) ||
                            (g.Player1Id == userBId && g.Player2Id == userAId)));
    }

    public async Task AddShipsAsync(IEnumerable<BattleshipShip> ships)
    {
        await _dbContext.BattleshipShips.AddRangeAsync(ships);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddAttackAsync(BattleshipAttack attack)
    {
        await _dbContext.BattleshipAttacks.AddAsync(attack);
        await _dbContext.SaveChangesAsync();
    }
}
