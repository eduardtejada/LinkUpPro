namespace LinkUpPro.Core.Application.ViewModels.Comment;

public class CommentViewModel
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUserName { get; set; } = string.Empty;
    public string? AuthorProfilePictureUrl { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? ParentCommentId { get; set; }
    public List<CommentViewModel> Replies { get; set; } = new();

    public bool CanDelete { get; set; }
}
