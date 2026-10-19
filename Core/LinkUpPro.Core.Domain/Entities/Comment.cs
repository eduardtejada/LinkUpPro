using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities;

public class Comment : BaseEntity
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    // Soporte para respuestas anidadas (hilos)
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
