using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
{
    public FriendshipRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Friendship?> GetFriendshipBetweenUsersAsync(string userAId, string userBId)
    {
        return await _dbContext.Friendships
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .FirstOrDefaultAsync(f => 
                (f.UserAId == userAId && f.UserBId == userBId) ||
                (f.UserAId == userBId && f.UserBId == userAId));
    }

    public async Task<IReadOnlyList<Friendship>> GetActiveFriendshipsForUserAsync(string userId)
    {
        return await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => (f.UserAId == userId || f.UserBId == userId) && f.Status == FriendshipStatus.Active)
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetActiveFriendsAsync(string userId)
    {
        var friendships = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => (f.UserAId == userId || f.UserBId == userId) && f.Status == FriendshipStatus.Active)
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .ToListAsync();

        return friendships
            .Select(f => f.UserAId == userId ? f.UserB : f.UserA)
            .DistinctBy(u => u.Id)
            .ToList();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetMutualFriendsAsync(string userAId, string userBId)
    {
        var friendsOfA = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => (f.UserAId == userAId || f.UserBId == userAId) && f.Status == FriendshipStatus.Active)
            .Select(f => f.UserAId == userAId ? f.UserBId : f.UserAId)
            .ToListAsync();

        var friendsOfB = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => (f.UserAId == userBId || f.UserBId == userBId) && f.Status == FriendshipStatus.Active)
            .Select(f => f.UserAId == userBId ? f.UserBId : f.UserAId)
            .ToListAsync();

        // Intersección excluyendo a la propia pareja
        var mutualIds = friendsOfA.Intersect(friendsOfB)
            .Where(id => id != userAId && id != userBId)
            .ToList();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => mutualIds.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<FriendRequest?> GetPendingRequestBetweenUsersAsync(string userAId, string userBId)
    {
        return await _dbContext.FriendRequests
            .FirstOrDefaultAsync(r => r.Status == FriendRequestStatus.Pending &&
                ((r.SenderId == userAId && r.ReceiverId == userBId) ||
                 (r.SenderId == userBId && r.ReceiverId == userAId)));
    }

    public async Task<IReadOnlyList<FriendRequest>> GetPendingReceivedRequestsAsync(string userId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending)
            .Include(r => r.Sender)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FriendRequest>> GetPendingSentRequestsAsync(string userId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => r.SenderId == userId && r.Status == FriendRequestStatus.Pending)
            .Include(r => r.Receiver)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FriendRequest>> GetRequestHistoryAsync(string userId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => (r.SenderId == userId || r.ReceiverId == userId) && 
                        r.Status != FriendRequestStatus.Pending && 
                        !r.IsHidden)
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .OrderByDescending(r => r.RespondedAt ?? r.CreatedAt)
            .ToListAsync();
    }

    public async Task<FriendRequest?> GetRequestByIdAsync(int requestId)
    {
        return await _dbContext.FriendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task AddRequestAsync(FriendRequest request)
    {
        await _dbContext.FriendRequests.AddAsync(request);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRequestAsync(FriendRequest request)
    {
        _dbContext.Entry(request).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
    {
        var normalized = userName.ToUpperInvariant();
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
    }
}
