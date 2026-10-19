using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class Friendship : BaseEntity
{
    // Una única relación por pareja de usuarios.
    // En la base de datos se garantiza con índice único y orden consistente (UserA / UserB)
    public string UserAId { get; set; } = string.Empty;
    public ApplicationUser UserA { get; set; } = null!;

    public string UserBId { get; set; } = string.Empty;
    public ApplicationUser UserB { get; set; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
