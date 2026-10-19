using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Comment;

public class SaveCommentViewModel
{
    [Required]
    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }

    [Required(ErrorMessage = "El comentario no puede estar vacío.")]
    [StringLength(1500, ErrorMessage = "El comentario no puede exceder los 1500 caracteres.")]
    public string Content { get; set; } = string.Empty;
}
