using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IPostService
{
    Task<IReadOnlyList<PostViewModel>> GetHomeFeedAsync(string currentUserId, PostFilterViewModel? filter = null);

    Task<IReadOnlyList<PostViewModel>> GetFriendsFeedAsync(string currentUserId, PostFilterViewModel? filter = null);

    Task<PostViewModel?> GetByIdAsync(int postId, string currentUserId);

    Task<SavePostViewModel?> GetForEditAsync(int postId, string currentUserId);

    Task<(bool Success, string? ErrorMessage, int PostId)> CreatePostAsync(
        string currentUserId, 
        SavePostViewModel vm, 
        Stream? imageStream, 
        string? imageFileName);

    Task<(bool Success, string? ErrorMessage)> UpdatePostAsync(
        string currentUserId, 
        SavePostViewModel vm, 
        Stream? imageStream, 
        string? imageFileName);

    Task<bool> DeletePostAsync(int postId, string currentUserId);

    Task<bool> ToggleCommentsAsync(int postId, string currentUserId);

    Task<(bool Success, string? ErrorMessage)> AddCommentAsync(string currentUserId, SaveCommentViewModel vm);

    Task<bool> DeleteCommentAsync(int commentId, string currentUserId);

    Task<bool> ToggleReactionAsync(int postId, string currentUserId, ReactionType type);
}
