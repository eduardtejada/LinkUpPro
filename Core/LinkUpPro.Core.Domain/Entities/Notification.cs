using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Domain.Entities;

public class Notification : BaseEntity
{
    // Destinatario de la notificación
    public string RecipientId { get; set; } = string.Empty;
    public ApplicationUser Recipient { get; set; } = null!;

    // Usuario que originó la acción (quien comentó, reaccionó, etc.)
    public string ActorId { get; set; } = string.Empty;
    public ApplicationUser Actor { get; set; } = null!;

    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
