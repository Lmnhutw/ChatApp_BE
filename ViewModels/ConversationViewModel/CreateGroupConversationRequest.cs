using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class CreateGroupConversationRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public IReadOnlyList<string> MemberUserIds { get; set; } = [];
}
