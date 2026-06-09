using ChatApp_BE.ViewModels.ConversationViewModel;

namespace ChatApp_BE.ViewModels.Realtime;

public sealed class MessageReactionEvent
{
    public Guid ConversationId { get; set; }

    public Guid MessageId { get; set; }

    public MessageReactionResponse? Reaction { get; set; }

    public string? RemovedReaction { get; set; }

    public string UserId { get; set; } = string.Empty;
}
