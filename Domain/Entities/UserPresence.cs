using ChatApp_BE.Domain.Enums;
using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class UserPresence
{
    public string UserId { get; set; } = string.Empty;

    public UserPresenceStatus Status { get; set; } = UserPresenceStatus.Offline;

    public int ConnectionCount { get; set; }

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
