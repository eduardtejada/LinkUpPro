using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Post;

public class SavePostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El contenido de la publicación es obligatorio.")]
    [StringLength(5000, ErrorMessage = "El contenido no puede exceder los 5000 caracteres.")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Tipo de Multimedia")]
    public PostMediaType MediaType { get; set; } = PostMediaType.None;

    [Url(ErrorMessage = "Ingrese una URL válida.")]
    [Display(Name = "Enlace de YouTube")]
    public string? YouTubeUrl { get; set; }

    [Display(Name = "Privacidad")]
    public PostPrivacy Privacy { get; set; } = PostPrivacy.FriendsOnly;

    [Display(Name = "Desactivar comentarios en esta publicación")]
    public bool AreCommentsDisabled { get; set; } = false;

    public string? ExistingMediaUrl { get; set; }
    public bool RemoveExistingMedia { get; set; } = false;
}
