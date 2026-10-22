using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId);

    Task<int> GetUnreadCountAsync(string userId);

    Task<int> GetPendingFriendRequestsCountAsync(string userId);

    Task MarkAsReadAsync(int notificationId, string userId);

    Task MarkAllAsReadAsync(string userId);
}
