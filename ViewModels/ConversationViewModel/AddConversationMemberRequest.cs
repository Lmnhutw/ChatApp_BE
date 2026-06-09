using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class AddConversationMemberRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
