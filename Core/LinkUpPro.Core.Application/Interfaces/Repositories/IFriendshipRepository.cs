using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.Repositories;

public interface IFriendshipRepository : IGenericRepository<Friendship>
{
    Task<Friendship?> GetFriendshipBetweenUsersAsync(string userAId, string userBId);

    Task<IReadOnlyList<Friendship>> GetActiveFriendshipsForUserAsync(string userId);

    Task<IReadOnlyList<ApplicationUser>> GetActiveFriendsAsync(string userId);

    Task<IReadOnlyList<ApplicationUser>> GetMutualFriendsAsync(string userAId, string userBId);

    Task<FriendRequest?> GetPendingRequestBetweenUsersAsync(string userAId, string userBId);

    Task<IReadOnlyList<FriendRequest>> GetPendingReceivedRequestsAsync(string userId);

    Task<IReadOnlyList<FriendRequest>> GetPendingSentRequestsAsync(string userId);

    Task<IReadOnlyList<FriendRequest>> GetRequestHistoryAsync(string userId);

    Task<FriendRequest?> GetRequestByIdAsync(int requestId);

    Task AddRequestAsync(FriendRequest request);

    Task UpdateRequestAsync(FriendRequest request);

    Task<ApplicationUser?> GetUserByUserNameAsync(string userName);
}
