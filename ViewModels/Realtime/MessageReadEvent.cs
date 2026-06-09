using ChatApp_BE.ViewModels.ConversationViewModel;

namespace ChatApp_BE.ViewModels.Realtime;

public sealed class MessageReadEvent
{
    public Guid ConversationId { get; set; }

    public MessageReadReceiptResponse Receipt { get; set; } = null!;
}
