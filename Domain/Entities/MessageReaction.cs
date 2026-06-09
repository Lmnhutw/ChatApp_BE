using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class MessageReaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Reaction { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatMessage Message { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
