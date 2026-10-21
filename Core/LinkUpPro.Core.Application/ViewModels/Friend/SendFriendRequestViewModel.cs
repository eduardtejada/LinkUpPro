using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Friend;

public class SendFriendRequestViewModel
{
    [Required(ErrorMessage = "Debe ingresar el nombre de usuario.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;
}
