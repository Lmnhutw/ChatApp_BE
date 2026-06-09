namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class SearchMessagesResponse
{
    public IReadOnlyList<MessageResponse> Items { get; set; } = [];
}
