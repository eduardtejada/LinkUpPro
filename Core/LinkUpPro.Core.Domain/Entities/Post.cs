using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class Post : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public PostMediaType MediaType { get; set; } = PostMediaType.None;
    public string? MediaUrl { get; set; }

    public PostPrivacy Privacy { get; set; } = PostPrivacy.FriendsOnly;
    public bool AreCommentsDisabled { get; set; } = false;
    public bool IsEdited { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
