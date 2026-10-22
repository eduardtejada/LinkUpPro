using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Notification;

public class NotificationViewModel
{
    public int Id { get; set; }
    public string RecipientId { get; set; } = string.Empty;

    public string ActorId { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string ActorUserName { get; set; } = string.Empty;
    public string? ActorProfilePictureUrl { get; set; }

    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public string IconClass => Type switch
    {
        NotificationType.NewComment => "bi-chat-dots-fill text-primary",
        NotificationType.NewReply => "bi-reply-fill text-info",
        NotificationType.NewReaction => "bi-hand-thumbs-up-fill text-danger",
        NotificationType.FriendRequestReceived => "bi-person-plus-fill text-warning",
        NotificationType.FriendRequestAccepted => "bi-person-check-fill text-success",
        NotificationType.BattleshipTurn => "bi-crosshair text-danger",
        NotificationType.BattleshipGameOver => "bi-trophy-fill text-warning",
        _ => "bi-bell-fill text-secondary"
    };
}
