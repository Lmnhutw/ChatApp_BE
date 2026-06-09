namespace ChatApp_BE.ViewModels.Realtime;

public sealed class TypingIndicatorEvent
{
    public Guid ConversationId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool IsTyping { get; set; }
}
