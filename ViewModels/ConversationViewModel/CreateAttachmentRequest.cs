using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class CreateAttachmentRequest
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long SizeBytes { get; set; }

    [Required]
    [MaxLength(512)]
    public string StorageKey { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? PublicUrl { get; set; }
}
