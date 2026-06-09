using ChatApp_BE.Models;

namespace ChatApp_BE.Domain.Entities;

public class MessageAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? PublicUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatMessage Message { get; set; } = null!;

    public ApplicationUser UploadedByUser { get; set; } = null!;
}
