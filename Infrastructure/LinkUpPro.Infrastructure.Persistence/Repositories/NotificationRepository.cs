using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId)
            .Include(n => n.Actor)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _dbContext.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);
    }

    public async Task<int> GetPendingFriendRequestsCountAsync(string userId)
    {
        return await _dbContext.FriendRequests
            .CountAsync(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _dbContext.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
