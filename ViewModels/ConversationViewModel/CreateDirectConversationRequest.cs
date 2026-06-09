using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class CreateDirectConversationRequest
{
    [Required]
    public string OtherUserId { get; set; } = string.Empty;
}
