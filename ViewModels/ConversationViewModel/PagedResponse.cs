namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public DateTime? NextBefore { get; set; }
}
