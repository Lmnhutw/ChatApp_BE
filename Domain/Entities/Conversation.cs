using ChatApp_BE.Domain.Common;
using ChatApp_BE.Domain.Enums;
using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class Conversation : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public ConversationType Type { get; set; }

    public string? Title { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
