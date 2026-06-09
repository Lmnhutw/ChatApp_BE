using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.AuthViewModel;

public sealed class ResendVerificationEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
