using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ProfileViewModel;

public sealed class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(2048)]
    public string? Avatar { get; set; }
}
