using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.ViewModels.ConversationViewModel;

public sealed class AddReactionRequest
{
    [Required]
    [MaxLength(64)]
    public string Reaction { get; set; } = string.Empty;
}
