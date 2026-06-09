namespace ChatApp_BE.ViewModels.ProfileViewModel;

public sealed class UserBlockResponse
{
    public string BlockedUserId { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? Avatar { get; set; }

    public DateTime CreatedAt { get; set; }
}
