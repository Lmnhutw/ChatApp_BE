using ChatApp_BE.Domain.Enums;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class MessageResponse
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string? SenderFullName { get; set; }

    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; }

    public Guid? ReplyToMessageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
