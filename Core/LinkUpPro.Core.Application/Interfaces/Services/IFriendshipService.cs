using LinkUpPro.Core.Application.ViewModels.Friend;
using LinkUpPro.Core.Application.ViewModels.Post;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IFriendshipService
{
    Task<FriendsIndexViewModel> GetFriendsIndexAsync(string currentUserId, PostFilterViewModel? filter = null);

    Task<FriendRequestsPageViewModel> GetRequestsPageAsync(string currentUserId);

    Task<(bool Success, string? ErrorMessage)> SendRequestAsync(string senderId, string targetUserName);

    Task<(bool Success, string? ErrorMessage)> AcceptRequestAsync(int requestId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> CancelRequestAsync(int requestId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> HideRequestAsync(int requestId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> RemoveFriendAsync(string currentUserId, string friendId);

    Task<IReadOnlyList<MutualFriendDto>> GetMutualFriendsAsync(string currentUserId, string otherUserId);
}
