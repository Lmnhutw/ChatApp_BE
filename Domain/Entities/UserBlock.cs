using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class UserBlock
{
    public string BlockerUserId { get; set; } = string.Empty;

    public string BlockedUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser BlockerUser { get; set; } = null!;

    public ApplicationUser BlockedUser { get; set; } = null!;
}
