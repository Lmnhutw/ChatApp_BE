namespace ChatApp_BE.ViewModels.AuthViewModel;

public sealed class UserResponse
{
    public string Id { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? FullName { get; set; }

    public string? Avatar { get; set; }
}
