namespace LinkUpPro.Core.Application.ViewModels.Friend;

public class FriendRequestsPageViewModel
{
    public List<FriendRequestViewModel> ReceivedPending { get; set; } = new();
    public List<FriendRequestViewModel> SentPending { get; set; } = new();
    public List<FriendRequestViewModel> History { get; set; } = new();
    public SendFriendRequestViewModel SendModel { get; set; } = new();
}
