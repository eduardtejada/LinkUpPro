using LinkUpPro.Core.Application.ViewModels.Post;

namespace LinkUpPro.Core.Application.ViewModels.Friend;

public class FriendsIndexViewModel
{
    public List<FriendViewModel> ActiveFriends { get; set; } = new();
    public int ActiveFriendsCount => ActiveFriends.Count;

    public IReadOnlyList<PostViewModel> FriendsPosts { get; set; } = new List<PostViewModel>();
    public int AvailablePostsCount => FriendsPosts.Count;

    public PostFilterViewModel Filter { get; set; } = new();
}
