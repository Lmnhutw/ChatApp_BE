using ChatApp_BE.Extensions;
using ChatApp_BE.Services.Conversations;
using ChatApp_BE.ViewModels.ConversationViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _conversationService.GetConversationsAsync(User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        var result = await _conversationService.GetConversationAsync(conversationId, User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectConversation(CreateDirectConversationRequest request)
    {
        var result = await _conversationService.CreateDirectConversationAsync(User.GetRequiredUserId(), request);
        return result.Status == ConversationServiceResultStatus.Success
            ? CreatedAtAction(nameof(GetConversation), new { conversationId = ((ConversationResponse)result.Value!).Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationRequest request)
    {
        var result = await _conversationService.CreateGroupConversationAsync(User.GetRequiredUserId(), request);
        return result.Status == ConversationServiceResultStatus.Success
            ? CreatedAtAction(nameof(GetConversation), new { conversationId = ((ConversationResponse)result.Value!).Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpGet("{conversationId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid conversationId)
    {
        var result = await _conversationService.GetMembersAsync(conversationId, User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPost("{conversationId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid conversationId, AddConversationMemberRequest request)
    {
        var result = await _conversationService.AddMemberAsync(conversationId, User.GetRequiredUserId(), request);
        return ToActionResult(result);
    }

    [HttpDelete("{conversationId:guid}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(Guid conversationId, string memberUserId)
    {
        var result = await _conversationService.RemoveMemberAsync(conversationId, User.GetRequiredUserId(), memberUserId);
        return ToActionResult(result);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] DateTime? before, [FromQuery] int take = 50)
    {
        var result = await _conversationService.GetMessagesAsync(conversationId, User.GetRequiredUserId(), before, take);
        return ToActionResult(result);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, SendMessageRequest request)
    {
        var result = await _conversationService.SendMessageAsync(conversationId, User.GetRequiredUserId(), request);
        return result.Status == ConversationServiceResultStatus.Success
            ? CreatedAtAction(nameof(GetMessages), new { conversationId }, result.Value)
            : ToActionResult(result);
    }

    [HttpPut("{conversationId:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> UpdateMessage(Guid conversationId, Guid messageId, UpdateMessageRequest request)
    {
        var result = await _conversationService.UpdateMessageAsync(conversationId, messageId, User.GetRequiredUserId(), request);
        return ToActionResult(result);
    }

    [HttpDelete("{conversationId:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid conversationId, Guid messageId)
    {
        var result = await _conversationService.DeleteMessageAsync(conversationId, messageId, User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPost("{conversationId:guid}/messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkMessageRead(Guid conversationId, Guid messageId)
    {
        var result = await _conversationService.MarkMessageReadAsync(conversationId, messageId, User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpGet("{conversationId:guid}/messages/{messageId:guid}/reactions")]
    public async Task<IActionResult> GetMessageReactions(Guid conversationId, Guid messageId)
    {
        var result = await _conversationService.GetMessageReactionsAsync(conversationId, messageId, User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPost("{conversationId:guid}/messages/{messageId:guid}/reactions")]
    public async Task<IActionResult> AddMessageReaction(Guid conversationId, Guid messageId, AddReactionRequest request)
    {
        var result = await _conversationService.AddMessageReactionAsync(conversationId, messageId, User.GetRequiredUserId(), request);
        return ToActionResult(result);
    }

    [HttpDelete("{conversationId:guid}/messages/{messageId:guid}/reactions/{reaction}")]
    public async Task<IActionResult> RemoveMessageReaction(Guid conversationId, Guid messageId, string reaction)
    {
        var result = await _conversationService.RemoveMessageReactionAsync(conversationId, messageId, User.GetRequiredUserId(), reaction);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(ConversationServiceResult result)
    {
        return result.Status switch
        {
            ConversationServiceResultStatus.Success => Ok(result.Value),
            ConversationServiceResultStatus.BadRequest => BadRequest(new { result.Message }),
            ConversationServiceResultStatus.Forbidden => Forbid(),
            ConversationServiceResultStatus.NotFound => NotFound(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
