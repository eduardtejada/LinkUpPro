namespace LinkUpPro.Core.Application.DTOs.Account;

public class AccountResponseDto
{
    public bool HasError { get; set; }
    public string? Error { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsLockedOut { get; set; }
    public bool IsAccountInactive { get; set; }
    public string? SuccessMessage { get; set; }
}
