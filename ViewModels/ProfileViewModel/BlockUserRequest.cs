using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ProfileViewModel;

public sealed class BlockUserRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
