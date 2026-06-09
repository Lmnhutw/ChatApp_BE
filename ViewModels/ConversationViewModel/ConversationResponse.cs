using ChatApp_BE.Domain.Enums;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class ConversationResponse
{
    public Guid Id { get; set; }

    public ConversationType Type { get; set; }

    public string? Title { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public IReadOnlyList<ConversationMemberResponse> Members { get; set; } = [];

    public MessageResponse? LastMessage { get; set; }
}
