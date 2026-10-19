using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Core.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Propiedades calculadas útiles
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Publicaciones e Interacciones
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    // Solicitudes de Amistad
    public ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();

    // Amistades (bidireccional en la BD mediante UserA o UserB)
    public ICollection<Friendship> FriendshipsAsUserA { get; set; } = new List<Friendship>();
    public ICollection<Friendship> FriendshipsAsUserB { get; set; } = new List<Friendship>();

    // Notificaciones
    public ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();
    public ICollection<Notification> NotificationsTriggered { get; set; } = new List<Notification>();

    // Partidas de Battleship
    public ICollection<BattleshipGame> BattleshipGamesAsPlayer1 { get; set; } = new List<BattleshipGame>();
    public ICollection<BattleshipGame> BattleshipGamesAsPlayer2 { get; set; } = new List<BattleshipGame>();
}
