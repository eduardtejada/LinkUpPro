using System.Text.RegularExpressions;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Post;

public class PostViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUserName { get; set; } = string.Empty;
    public string? AuthorProfilePictureUrl { get; set; }

    public string Content { get; set; } = string.Empty;
    public PostMediaType MediaType { get; set; }
    public string? MediaUrl { get; set; }

    public PostPrivacy Privacy { get; set; }
    public bool AreCommentsDisabled { get; set; }
    public bool IsEdited { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public ReactionType? UserReaction { get; set; }

    public bool CanModify { get; set; }

    public List<CommentViewModel> Comments { get; set; } = new();

    public string? YouTubeEmbedUrl
    {
        get
        {
            if (MediaType != PostMediaType.YouTube || string.IsNullOrWhiteSpace(MediaUrl))
            {
                return null;
            }

            var regex = new Regex(@"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/|youtube\.com\/shorts\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase);
            var match = regex.Match(MediaUrl);

            return match.Success 
                ? $"https://www.youtube.com/embed/{match.Groups[1].Value}" 
                : null;
        }
    }
}
