using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Notification;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationViewModel>> GetUserNotificationsAsync(string userId)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);

        return notifications.Select(n => new NotificationViewModel
        {
            Id = n.Id,
            RecipientId = n.RecipientId,
            ActorId = n.ActorId,
            ActorName = n.Actor != null ? n.Actor.FullName : "Usuario",
            ActorUserName = n.Actor != null ? (n.Actor.UserName ?? string.Empty) : string.Empty,
            ActorProfilePictureUrl = n.Actor?.ProfilePictureUrl,
            Type = n.Type,
            Message = n.Message,
            TargetUrl = n.TargetUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task<int> GetPendingFriendRequestsCountAsync(string userId)
    {
        return await _notificationRepository.GetPendingFriendRequestsCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, userId);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }
}
