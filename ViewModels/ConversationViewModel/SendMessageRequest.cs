using ChatApp_BE.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class SendMessageRequest
{
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public Guid? ReplyToMessageId { get; set; }
}
