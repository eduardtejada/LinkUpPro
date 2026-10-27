using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IGenericRepository<Reaction> _reactionRepository;
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly IFileStorageService _fileStorageService;

    public PostService(
        IPostRepository postRepository,
        IGenericRepository<Comment> commentRepository,
        IGenericRepository<Reaction> reactionRepository,
        IGenericRepository<Notification> notificationRepository,
        IFileStorageService fileStorageService)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _reactionRepository = reactionRepository;
        _notificationRepository = notificationRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<PostViewModel>> GetHomeFeedAsync(string currentUserId, PostFilterViewModel? filter = null)
    {
        var posts = await _postRepository.GetHomeFeedPostsAsync(currentUserId, filter);
        return posts.Select(p => MapToViewModel(p, currentUserId)).ToList();
    }

    public async Task<IReadOnlyList<PostViewModel>> GetFriendsFeedAsync(string currentUserId, PostFilterViewModel? filter = null)
    {
        var posts = await _postRepository.GetFriendsFeedPostsAsync(currentUserId, filter);
        return posts.Select(p => MapToViewModel(p, currentUserId)).ToList();
    }

    public async Task<PostViewModel?> GetByIdAsync(int postId, string currentUserId)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        return post == null ? null : MapToViewModel(post, currentUserId);
    }

    public async Task<SavePostViewModel?> GetForEditAsync(int postId, string currentUserId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null || post.UserId != currentUserId)
        {
            return null; // Validación estricta de propiedad
        }

        return new SavePostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            MediaType = post.MediaType,
            YouTubeUrl = post.MediaType == PostMediaType.YouTube ? post.MediaUrl : null,
            ExistingMediaUrl = post.MediaUrl,
            Privacy = post.Privacy,
            AreCommentsDisabled = post.AreCommentsDisabled
        };
    }

    public async Task<(bool Success, string? ErrorMessage, int PostId)> CreatePostAsync(
        string currentUserId, 
        SavePostViewModel vm, 
        Stream? imageStream, 
        string? imageFileName)
    {
        string? mediaUrl = null;
        var mediaType = vm.MediaType;

        // Validación de contenido condicional (exactamente 1 contenido multimedia o ninguno)
        if (mediaType == PostMediaType.Image)
        {
            if (imageStream == null || string.IsNullOrWhiteSpace(imageFileName))
            {
                return (false, "Debe seleccionar una imagen para este tipo de publicación.", 0);
            }

            if (!_fileStorageService.ValidateImage(imageFileName, imageStream.Length, out var error))
            {
                return (false, error, 0);
            }

            mediaUrl = await _fileStorageService.SaveFileAsync(imageStream, imageFileName, "posts");
        }
        else if (mediaType == PostMediaType.YouTube)
        {
            if (string.IsNullOrWhiteSpace(vm.YouTubeUrl))
            {
                return (false, "Debe ingresar el enlace de YouTube.", 0);
            }

            mediaUrl = vm.YouTubeUrl.Trim();
        }
        else
        {
            mediaType = PostMediaType.None;
            mediaUrl = null;
        }

        var post = new Post
        {
            UserId = currentUserId,
            Content = vm.Content.Trim(),
            MediaType = mediaType,
            MediaUrl = mediaUrl,
            Privacy = vm.Privacy,
            AreCommentsDisabled = vm.AreCommentsDisabled,
            IsEdited = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _postRepository.AddAsync(post);
        return (true, null, created.Id);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdatePostAsync(
        string currentUserId, 
        SavePostViewModel vm, 
        Stream? imageStream, 
        string? imageFileName)
    {
        var post = await _postRepository.GetByIdAsync(vm.Id);
        if (post == null || post.UserId != currentUserId)
        {
            return (false, "No tienes permisos para editar esta publicación.");
        }

        post.Content = vm.Content.Trim();
        post.Privacy = vm.Privacy;
        post.AreCommentsDisabled = vm.AreCommentsDisabled;
        post.IsEdited = true;
        post.UpdatedAt = DateTime.UtcNow;

        // Gestión de multimedia
        if (vm.RemoveExistingMedia)
        {
            if (post.MediaType == PostMediaType.Image && !string.IsNullOrWhiteSpace(post.MediaUrl))
            {
                _fileStorageService.DeleteFile(post.MediaUrl);
            }

            post.MediaType = PostMediaType.None;
            post.MediaUrl = null;
        }
        else if (vm.MediaType == PostMediaType.Image && imageStream != null && !string.IsNullOrWhiteSpace(imageFileName))
        {
            if (!_fileStorageService.ValidateImage(imageFileName, imageStream.Length, out var error))
            {
                return (false, error);
            }

            if (post.MediaType == PostMediaType.Image && !string.IsNullOrWhiteSpace(post.MediaUrl))
            {
                _fileStorageService.DeleteFile(post.MediaUrl);
            }

            post.MediaUrl = await _fileStorageService.SaveFileAsync(imageStream, imageFileName, "posts");
            post.MediaType = PostMediaType.Image;
        }
        else if (vm.MediaType == PostMediaType.YouTube)
        {
            if (string.IsNullOrWhiteSpace(vm.YouTubeUrl))
            {
                return (false, "Debe ingresar el enlace de YouTube.");
            }

            if (post.MediaType == PostMediaType.Image && !string.IsNullOrWhiteSpace(post.MediaUrl))
            {
                _fileStorageService.DeleteFile(post.MediaUrl);
            }

            post.MediaUrl = vm.YouTubeUrl.Trim();
            post.MediaType = PostMediaType.YouTube;
        }

        await _postRepository.UpdateAsync(post);
        return (true, null);
    }

    public async Task<bool> DeletePostAsync(int postId, string currentUserId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null || post.UserId != currentUserId)
        {
            return false;
        }

        if (post.MediaType == PostMediaType.Image && !string.IsNullOrWhiteSpace(post.MediaUrl))
        {
            _fileStorageService.DeleteFile(post.MediaUrl);
        }

        await _postRepository.DeleteAsync(post);
        return true;
    }

    public async Task<bool> ToggleCommentsAsync(int postId, string currentUserId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null || post.UserId != currentUserId)
        {
            return false;
        }

        post.AreCommentsDisabled = !post.AreCommentsDisabled;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<(bool Success, string? ErrorMessage)> AddCommentAsync(string currentUserId, SaveCommentViewModel vm)
    {
        var post = await _postRepository.GetByIdAsync(vm.PostId);
        if (post == null)
        {
            return (false, "Publicación no encontrada.");
        }

        if (post.AreCommentsDisabled)
        {
            return (false, "Los comentarios están desactivados en esta publicación.");
        }

        Comment? parentComment = null;
        if (vm.ParentCommentId.HasValue)
        {
            parentComment = await _commentRepository.GetByIdAsync(vm.ParentCommentId.Value);
            if (parentComment == null || parentComment.PostId != vm.PostId)
            {
                return (false, "El comentario al que intentas responder no existe en esta publicación.");
            }
        }

        var comment = new Comment
        {
            PostId = vm.PostId,
            UserId = currentUserId,
            ParentCommentId = vm.ParentCommentId,
            Content = vm.Content.Trim(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        // Crear notificación (excepto si el autor es el mismo usuario)
        if (parentComment != null)
        {
            // Notificar al autor del comentario padre
            if (parentComment.UserId != currentUserId)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    RecipientId = parentComment.UserId,
                    ActorId = currentUserId,
                    Type = NotificationType.NewReply,
                    Message = "respondió a tu comentario.",
                    TargetUrl = $"/Home/Index#comment-{comment.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            // Notificar al dueño de la publicación
            if (post.UserId != currentUserId)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    RecipientId = post.UserId,
                    ActorId = currentUserId,
                    Type = NotificationType.NewComment,
                    Message = "comentó en tu publicación.",
                    TargetUrl = $"/Home/Index#comment-{comment.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return (true, null);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, string currentUserId)
    {
        var commentList = await _commentRepository.GetAsync(
            c => c.Id == commentId,
            includes: new() { c => c.Post, c => c.Replies });

        var comment = commentList.FirstOrDefault();
        if (comment == null)
        {
            return false;
        }

        // Permiso: autor del comentario o dueño de la publicación
        if (comment.UserId != currentUserId && comment.Post.UserId != currentUserId)
        {
            return false;
        }

        if (comment.Replies.Any())
        {
            comment.Content = "Este comentario fue eliminado";
            comment.IsDeleted = true;
            await _commentRepository.UpdateAsync(comment);
        }
        else
        {
            await _commentRepository.DeleteAsync(comment);
        }

        return true;
    }

    public async Task<bool> ToggleReactionAsync(int postId, string currentUserId, ReactionType type)
    {
        var existingReaction = (await _reactionRepository.GetAsync(
            r => r.PostId == postId && r.UserId == currentUserId)).FirstOrDefault();

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return false;
        }

        if (existingReaction != null)
        {
            if (existingReaction.Type == type)
            {
                await _reactionRepository.DeleteAsync(existingReaction);
                return true;
            }

            existingReaction.Type = type;
            await _reactionRepository.UpdateAsync(existingReaction);
            return true;
        }

        // Crear nueva reacción
        var reaction = new Reaction
        {
            PostId = postId,
            UserId = currentUserId,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        await _reactionRepository.AddAsync(reaction);

        // Notificar al autor del post (excepto si es el mismo usuario)
        if (post.UserId != currentUserId)
        {
            var reactionText = type == ReactionType.Like ? "le dio Me gusta a" : "le dio No me gusta a";
            await _notificationRepository.AddAsync(new Notification
            {
                RecipientId = post.UserId,
                ActorId = currentUserId,
                Type = NotificationType.NewReaction,
                Message = $"{reactionText} tu publicación.",
                TargetUrl = $"/Home/Index#post-{postId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        return true;
    }

    private static PostViewModel MapToViewModel(Post post, string currentUserId)
    {
        var vm = new PostViewModel
        {
            Id = post.Id,
            UserId = post.UserId,
            AuthorName = post.User != null ? post.User.FullName : "Usuario",
            AuthorUserName = post.User != null ? (post.User.UserName ?? string.Empty) : string.Empty,
            AuthorProfilePictureUrl = post.User?.ProfilePictureUrl,
            Content = post.Content,
            MediaType = post.MediaType,
            MediaUrl = post.MediaUrl,
            Privacy = post.Privacy,
            AreCommentsDisabled = post.AreCommentsDisabled,
            IsEdited = post.IsEdited,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            CanModify = (post.UserId == currentUserId),
            LikesCount = post.Reactions.Count(r => r.Type == ReactionType.Like),
            DislikesCount = post.Reactions.Count(r => r.Type == ReactionType.Dislike),
            UserReaction = post.Reactions.FirstOrDefault(r => r.UserId == currentUserId)?.Type
        };

        // Construir árbol de comentarios jerárquicos (raíz y respuestas anidadas)
        var allComments = post.Comments.Select(c => new CommentViewModel
        {
            Id = c.Id,
            PostId = c.PostId,
            UserId = c.UserId,
            AuthorName = c.User != null ? c.User.FullName : "Usuario",
            AuthorUserName = c.User != null ? (c.User.UserName ?? string.Empty) : string.Empty,
            AuthorProfilePictureUrl = c.User?.ProfilePictureUrl,
            Content = c.Content,
            IsDeleted = c.IsDeleted,
            CreatedAt = c.CreatedAt,
            ParentCommentId = c.ParentCommentId,
            CanDelete = (c.UserId == currentUserId || post.UserId == currentUserId)
        }).ToList();

        // Anidar respuestas
        var lookup = allComments.ToLookup(c => c.ParentCommentId);
        foreach (var comment in allComments)
        {
            comment.Replies = lookup[comment.Id].OrderBy(r => r.CreatedAt).ToList();
        }

        // Solo los comentarios de primer nivel van a la lista principal
        vm.Comments = lookup[null].OrderBy(c => c.CreatedAt).ToList();

        return vm;
    }
}
