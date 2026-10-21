using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Friend;

public class FriendRequestViewModel
{
    public int Id { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderUserName { get; set; } = string.Empty;
    public string? SenderProfilePictureUrl { get; set; }

    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverUserName { get; set; } = string.Empty;
    public string? ReceiverProfilePictureUrl { get; set; }

    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    public bool IsSender { get; set; }
    public bool IsHidden { get; set; }
}
