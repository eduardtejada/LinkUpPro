using LinkUpPro.Core.Application.ViewModels.Notification;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationViewModel>> GetUserNotificationsAsync(string userId);

    Task<int> GetUnreadCountAsync(string userId);

    Task<int> GetPendingFriendRequestsCountAsync(string userId);

    Task MarkAsReadAsync(int notificationId, string userId);

    Task MarkAllAsReadAsync(string userId);
}
