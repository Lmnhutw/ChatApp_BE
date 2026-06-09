namespace ChatApp_BE.ViewModels.ProfileViewModel;

public sealed class UserProfileResponse
{
    public string Id { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? FullName { get; set; }

    public string? DisplayName { get; set; }

    public string? Avatar { get; set; }
}
