using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class UpdateMessageRequest
{
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}
