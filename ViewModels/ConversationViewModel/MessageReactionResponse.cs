namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class MessageReactionResponse
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string Reaction { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
