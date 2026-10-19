using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<string>> GetActiveFriendIdsAsync(string userId)
    {
        var friendsA = await _dbContext.Friendships
            .Where(f => f.UserAId == userId && f.Status == FriendshipStatus.Active)
            .Select(f => f.UserBId)
            .ToListAsync();

        var friendsB = await _dbContext.Friendships
            .Where(f => f.UserBId == userId && f.Status == FriendshipStatus.Active)
            .Select(f => f.UserAId)
            .ToListAsync();

        return friendsA.Concat(friendsB).Distinct().ToList();
    }

    public async Task<IReadOnlyList<Post>> GetHomeFeedPostsAsync(string userId, PostFilterViewModel? filter)
    {
        var query = _dbContext.Posts
            .AsNoTracking()
            .Where(p => p.UserId == userId);

        query = ApplyPostIncludes(query);
        query = ApplyFilters(query, filter);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyList<Post>> GetFriendsFeedPostsAsync(string currentUserId, PostFilterViewModel? filter)
    {
        var friendIds = await GetActiveFriendIdsAsync(currentUserId);

        var query = _dbContext.Posts
            .AsNoTracking()
            .Where(p => friendIds.Contains(p.UserId) && p.Privacy == PostPrivacy.FriendsOnly);

        query = ApplyPostIncludes(query);
        query = ApplyFilters(query, filter);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Post?> GetPostWithDetailsAsync(int postId)
    {
        var query = _dbContext.Posts.Where(p => p.Id == postId);
        query = ApplyPostIncludes(query);

        return await query.FirstOrDefaultAsync();
    }

    private static IQueryable<Post> ApplyPostIncludes(IQueryable<Post> query)
    {
        return query
            .Include(p => p.User)
            .Include(p => p.Reactions)
                .ThenInclude(r => r.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User);
    }

    private static IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostFilterViewModel? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim();
            query = query.Where(p => p.Content.Contains(search));
        }

        if (filter.MediaType.HasValue)
        {
            query = query.Where(p => p.MediaType == filter.MediaType.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.CreatedAt <= endOfDay);
        }

        if (filter.EditStatus == EditFilterStatus.Edited)
        {
            query = query.Where(p => p.IsEdited);
        }
        else if (filter.EditStatus == EditFilterStatus.NotEdited)
        {
            query = query.Where(p => !p.IsEdited);
        }

        return query;
    }
}
