namespace LinkUpPro.Core.Application.ViewModels.Friend;

public class FriendViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime FriendshipDate { get; set; }
    public int MutualFriendsCount { get; set; }
    public List<MutualFriendDto> MutualFriends { get; set; } = new();
}
