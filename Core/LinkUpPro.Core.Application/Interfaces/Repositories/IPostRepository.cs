using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.Repositories;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<IReadOnlyList<Post>> GetHomeFeedPostsAsync(string userId, PostFilterViewModel? filter);

    Task<IReadOnlyList<Post>> GetFriendsFeedPostsAsync(string currentUserId, PostFilterViewModel? filter);

    Task<Post?> GetPostWithDetailsAsync(int postId);

    Task<IReadOnlyList<string>> GetActiveFriendIdsAsync(string userId);
}
