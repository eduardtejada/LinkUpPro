using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Friend;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.Services;

public class FriendshipService : IFriendshipService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IPostService _postService;
    private readonly IGenericRepository<Notification> _notificationRepository;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IPostService postService,
        IGenericRepository<Notification> notificationRepository)
    {
        _friendshipRepository = friendshipRepository;
        _postService = postService;
        _notificationRepository = notificationRepository;
    }

    public async Task<FriendsIndexViewModel> GetFriendsIndexAsync(string currentUserId, PostFilterViewModel? filter = null)
    {
        var activeFriends = await _friendshipRepository.GetActiveFriendsAsync(currentUserId);
        var activeFriendships = await _friendshipRepository.GetActiveFriendshipsForUserAsync(currentUserId);

        var friendVms = new List<FriendViewModel>();

        foreach (var friend in activeFriends)
        {
            var friendship = activeFriendships.FirstOrDefault(f => 
                (f.UserAId == friend.Id && f.UserBId == currentUserId) || 
                (f.UserBId == friend.Id && f.UserAId == currentUserId));

            var mutuals = await _friendshipRepository.GetMutualFriendsAsync(currentUserId, friend.Id);

            friendVms.Add(new FriendViewModel
            {
                UserId = friend.Id,
                FullName = friend.FullName,
                UserName = friend.UserName ?? string.Empty,
                ProfilePictureUrl = friend.ProfilePictureUrl,
                FriendshipDate = friendship?.CreatedAt ?? DateTime.UtcNow,
                MutualFriendsCount = mutuals.Count,
                MutualFriends = mutuals.Select(m => new MutualFriendDto
                {
                    UserId = m.Id,
                    FullName = m.FullName,
                    UserName = m.UserName ?? string.Empty,
                    ProfilePictureUrl = m.ProfilePictureUrl
                }).ToList()
            });
        }

        var friendsPosts = await _postService.GetFriendsFeedAsync(currentUserId, filter);

        return new FriendsIndexViewModel
        {
            ActiveFriends = friendVms.OrderBy(f => f.FullName).ToList(),
            FriendsPosts = friendsPosts,
            Filter = filter ?? new PostFilterViewModel()
        };
    }

    public async Task<FriendRequestsPageViewModel> GetRequestsPageAsync(string currentUserId)
    {
        var received = await _friendshipRepository.GetPendingReceivedRequestsAsync(currentUserId);
        var sent = await _friendshipRepository.GetPendingSentRequestsAsync(currentUserId);
        var history = await _friendshipRepository.GetRequestHistoryAsync(currentUserId);

        return new FriendRequestsPageViewModel
        {
            ReceivedPending = received.Select(r => MapToRequestVm(r, currentUserId)).ToList(),
            SentPending = sent.Select(r => MapToRequestVm(r, currentUserId)).ToList(),
            History = history.Select(r => MapToRequestVm(r, currentUserId)).ToList(),
            SendModel = new SendFriendRequestViewModel()
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> SendRequestAsync(string senderId, string targetUserName)
    {
        if (string.IsNullOrWhiteSpace(targetUserName))
        {
            return (false, "Debe ingresar el nombre de usuario.");
        }

        var targetUser = await _friendshipRepository.GetUserByUserNameAsync(targetUserName.Trim());
        if (targetUser == null)
        {
            return (false, $"El usuario '{targetUserName}' no existe.");
        }

        // Rúbrica: Prevención estricta de auto-solicitudes
        if (targetUser.Id == senderId)
        {
            return (false, "No puedes enviarte una solicitud de amistad a ti mismo.");
        }

        // Rúbrica: Verificar si ya existe una amistad activa
        var existingFriendship = await _friendshipRepository.GetFriendshipBetweenUsersAsync(senderId, targetUser.Id);
        if (existingFriendship != null && existingFriendship.Status == FriendshipStatus.Active)
        {
            return (false, $"Ya eres amigo de {targetUser.FullName} (@{targetUser.UserName}).");
        }

        // Rúbrica: Prevención de solicitudes duplicadas en ambas direcciones
        var pendingRequest = await _friendshipRepository.GetPendingRequestBetweenUsersAsync(senderId, targetUser.Id);
        if (pendingRequest != null)
        {
            if (pendingRequest.SenderId == senderId)
            {
                return (false, "Ya tienes una solicitud pendiente enviada a este usuario.");
            }

            return (false, $"Este usuario ya te envió una solicitud previamente. Puedes aceptarla en tus solicitudes recibidas.");
        }

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = targetUser.Id,
            Status = FriendRequestStatus.Pending,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow
        };

        await _friendshipRepository.AddRequestAsync(request);

        // Notificar al destinatario
        await _notificationRepository.AddAsync(new Notification
        {
            RecipientId = targetUser.Id,
            ActorId = senderId,
            Type = NotificationType.FriendRequestReceived,
            Message = "te envió una solicitud de amistad.",
            TargetUrl = "/Friends/Requests",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> AcceptRequestAsync(int requestId, string currentUserId)
    {
        var request = await _friendshipRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.ReceiverId != currentUserId || request.Status != FriendRequestStatus.Pending)
        {
            return (false, "La solicitud no es válida o ya ha sido procesada.");
        }

        request.Status = FriendRequestStatus.Accepted;
        request.RespondedAt = DateTime.UtcNow;
        await _friendshipRepository.UpdateRequestAsync(request);

        // Rúbrica: "Una única relación para cada pareja de usuarios. Al aceptar solicitud previa eliminada, se reactiva la existente."
        var existingFriendship = await _friendshipRepository.GetFriendshipBetweenUsersAsync(request.SenderId, request.ReceiverId);

        if (existingFriendship != null)
        {
            // Reactivación
            existingFriendship.Status = FriendshipStatus.Active;
            existingFriendship.UpdatedAt = DateTime.UtcNow;
            await _friendshipRepository.UpdateAsync(existingFriendship);
        }
        else
        {
            // Creación asegurando orden canónico
            var userA = string.Compare(request.SenderId, request.ReceiverId, StringComparison.Ordinal) < 0 
                ? request.SenderId 
                : request.ReceiverId;

            var userB = userA == request.SenderId ? request.ReceiverId : request.SenderId;

            var friendship = new Friendship
            {
                UserAId = userA,
                UserBId = userB,
                Status = FriendshipStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _friendshipRepository.AddAsync(friendship);
        }

        // Notificar al solicitante
        await _notificationRepository.AddAsync(new Notification
        {
            RecipientId = request.SenderId,
            ActorId = currentUserId,
            Type = NotificationType.FriendRequestAccepted,
            Message = "aceptó tu solicitud de amistad.",
            TargetUrl = "/Friends/Index",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId, string currentUserId)
    {
        var request = await _friendshipRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.ReceiverId != currentUserId || request.Status != FriendRequestStatus.Pending)
        {
            return (false, "La solicitud no es válida o ya ha sido procesada.");
        }

        request.Status = FriendRequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
        await _friendshipRepository.UpdateRequestAsync(request);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CancelRequestAsync(int requestId, string currentUserId)
    {
        var request = await _friendshipRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.SenderId != currentUserId || request.Status != FriendRequestStatus.Pending)
        {
            return (false, "La solicitud no es válida o ya ha sido procesada.");
        }

        request.Status = FriendRequestStatus.Cancelled;
        request.RespondedAt = DateTime.UtcNow;
        await _friendshipRepository.UpdateRequestAsync(request);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> HideRequestAsync(int requestId, string currentUserId)
    {
        var request = await _friendshipRepository.GetRequestByIdAsync(requestId);
        if (request == null || (request.SenderId != currentUserId && request.ReceiverId != currentUserId))
        {
            return (false, "Solicitud no encontrada.");
        }

        // Rúbrica: Ocultar solicitudes del historial sin eliminarlas físicamente
        request.IsHidden = true;
        await _friendshipRepository.UpdateRequestAsync(request);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RemoveFriendAsync(string currentUserId, string friendId)
    {
        var friendship = await _friendshipRepository.GetFriendshipBetweenUsersAsync(currentUserId, friendId);
        if (friendship == null || friendship.Status != FriendshipStatus.Active)
        {
            return (false, "No se encontró una amistad activa con este usuario.");
        }

        // Rúbrica: Eliminación lógica y bidireccional. Conserva interacciones históricas y partidas activas de Battleship.
        friendship.Status = FriendshipStatus.Deleted;
        friendship.UpdatedAt = DateTime.UtcNow;
        await _friendshipRepository.UpdateAsync(friendship);

        return (true, null);
    }

    public async Task<IReadOnlyList<MutualFriendDto>> GetMutualFriendsAsync(string currentUserId, string otherUserId)
    {
        var mutuals = await _friendshipRepository.GetMutualFriendsAsync(currentUserId, otherUserId);
        return mutuals.Select(m => new MutualFriendDto
        {
            UserId = m.Id,
            FullName = m.FullName,
            UserName = m.UserName ?? string.Empty,
            ProfilePictureUrl = m.ProfilePictureUrl
        }).ToList();
    }

    private static FriendRequestViewModel MapToRequestVm(FriendRequest r, string currentUserId)
    {
        return new FriendRequestViewModel
        {
            Id = r.Id,
            SenderId = r.SenderId,
            SenderName = r.Sender != null ? r.Sender.FullName : "Usuario",
            SenderUserName = r.Sender != null ? (r.Sender.UserName ?? string.Empty) : string.Empty,
            SenderProfilePictureUrl = r.Sender?.ProfilePictureUrl,
            ReceiverId = r.ReceiverId,
            ReceiverName = r.Receiver != null ? r.Receiver.FullName : "Usuario",
            ReceiverUserName = r.Receiver != null ? (r.Receiver.UserName ?? string.Empty) : string.Empty,
            ReceiverProfilePictureUrl = r.Receiver?.ProfilePictureUrl,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            RespondedAt = r.RespondedAt,
            IsSender = (r.SenderId == currentUserId),
            IsHidden = r.IsHidden
        };
    }
}
