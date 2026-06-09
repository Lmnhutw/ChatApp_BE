namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class MessageReadReceiptResponse
{
    public Guid MessageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public DateTime ReadAt { get; set; }
}
