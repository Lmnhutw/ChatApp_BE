namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class AttachmentResponse
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? PublicUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}
