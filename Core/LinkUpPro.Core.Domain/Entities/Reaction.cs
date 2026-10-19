using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class Reaction : BaseEntity
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ReactionType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
