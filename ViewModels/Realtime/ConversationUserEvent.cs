namespace ChatApp_BE.ViewModels.Realtime;

public sealed class ConversationUserEvent
{
    public Guid ConversationId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string ConnectionId { get; set; } = string.Empty;
}
