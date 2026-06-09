using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class MessageReadReceipt
{
    public Guid MessageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public ChatMessage Message { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
