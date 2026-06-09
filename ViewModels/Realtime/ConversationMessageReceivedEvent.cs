using ChatApp_BE.ViewModels.ConversationViewModel;

namespace ChatApp_BE.ViewModels.Realtime;

public sealed class ConversationMessageReceivedEvent
{
    public Guid ConversationId { get; set; }

    public MessageResponse Message { get; set; } = null!;
}
