using ChatApp_BE.Domain.Enums;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class ConversationMemberResponse
{
    public string UserId { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? Avatar { get; set; }

    public ConversationMemberRole Role { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }
}
