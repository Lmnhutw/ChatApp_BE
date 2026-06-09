using ChatApp_BE.Domain.Enums;

namespace ChatApp_BE.ViewModels.Realtime;

public sealed class UserPresenceEvent
{
    public string UserId { get; set; } = string.Empty;

    public UserPresenceStatus Status { get; set; }

    public int ConnectionCount { get; set; }

    public DateTime LastSeenAt { get; set; }
}
