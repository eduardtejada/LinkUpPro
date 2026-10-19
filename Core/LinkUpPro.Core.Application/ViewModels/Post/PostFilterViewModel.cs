using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Post;

public class PostFilterViewModel
{
    public string? SearchTerm { get; set; }
    public PostMediaType? MediaType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EditFilterStatus EditStatus { get; set; } = EditFilterStatus.All;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        MediaType.HasValue ||
        StartDate.HasValue ||
        EndDate.HasValue ||
        EditStatus != EditFilterStatus.All;
}
