using ChatApp_BE.Domain.Enums;
using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class ConversationMember
{
    public Guid ConversationId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ConversationMemberRole Role { get; set; } = ConversationMemberRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LeftAt { get; set; }

    public bool IsMuted { get; set; }

    public Guid? LastReadMessageId { get; set; }

    public DateTime? LastReadAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Conversation Conversation { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public ChatMessage? LastReadMessage { get; set; }
}
