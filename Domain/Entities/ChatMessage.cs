using ChatApp_BE.Domain.Common;
using ChatApp_BE.Domain.Enums;
using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class ChatMessage : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public Guid? ReplyToMessageId { get; set; }

    public string? EditedByUserId { get; set; }

    public string? DeletedByUserId { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Conversation Conversation { get; set; } = null!;

    public ApplicationUser Sender { get; set; } = null!;

    public ChatMessage? ReplyToMessage { get; set; }

    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();

    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();

    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();

    public ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();
}
