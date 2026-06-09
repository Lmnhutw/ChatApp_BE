using ChatApp_BE.Extensions;
using ChatApp_BE.Services.Conversations;
using ChatApp_BE.Services.Realtime;
using ChatApp_BE.ViewModels.ConversationViewModel;
using ChatApp_BE.ViewModels.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp_BE.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const string RealtimeErrorEventName = "RealtimeError";

    private readonly IConversationService _conversationService;
    private readonly IUserConnectionTracker _connectionTracker;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IConversationService conversationService,
        IUserConnectionTracker connectionTracker,
        ILogger<ChatHub> logger)
    {
        _conversationService = conversationService;
        _connectionTracker = connectionTracker;
        _logger = logger;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.GetConversationAsync(conversationId, userId);
        if (!result.Succeeded)
        {
            await SendErrorAsync("conversation_not_found", result.Message);
            return;
        }

        var groupName = GetConversationGroupName(conversationId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("ConversationJoined", result.Value);
        await Clients.GroupExcept(groupName, [Context.ConnectionId]).SendAsync("UserJoinedConversation", new ConversationUserEvent
        {
            ConversationId = conversationId,
            UserId = userId,
            ConnectionId = Context.ConnectionId
        });
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.GetConversationAsync(conversationId, userId);
        if (!result.Succeeded)
        {
            await SendErrorAsync("conversation_not_found", result.Message);
            return;
        }

        var groupName = GetConversationGroupName(conversationId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("ConversationLeft", new ConversationUserEvent
        {
            ConversationId = conversationId,
            UserId = userId,
            ConnectionId = Context.ConnectionId
        });
        await Clients.Group(groupName).SendAsync("UserLeftConversation", new ConversationUserEvent
        {
            ConversationId = conversationId,
            UserId = userId,
            ConnectionId = Context.ConnectionId
        });
    }

    public async Task SendConversationMessage(Guid conversationId, SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.SendMessageAsync(conversationId, userId, request);
        if (!result.Succeeded)
        {
            await SendErrorAsync("send_message_failed", result.Message);
            return;
        }

        var message = (MessageResponse)result.Value!;
        var messageEvent = new ConversationMessageReceivedEvent
        {
            ConversationId = conversationId,
            Message = message
        };

        await Clients.Group(GetConversationGroupName(conversationId)).SendAsync("MessageReceived", messageEvent);
        await Clients.Group(GetConversationGroupName(conversationId)).SendAsync("ReceiveMessage", new
        {
            sender = message.SenderFullName ?? message.SenderId,
            content = message.Content,
            timeStamp = message.CreatedAt.ToString("hh:mm tt")
        });
    }

    public async Task EditConversationMessage(Guid conversationId, Guid messageId, UpdateMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.UpdateMessageAsync(conversationId, messageId, userId, request);
        if (!result.Succeeded)
        {
            await SendErrorAsync("edit_message_failed", result.Message);
            return;
        }

        await Clients.Group(GetConversationGroupName(conversationId)).SendAsync("MessageUpdated", result.Value);
    }

    public async Task DeleteConversationMessage(Guid conversationId, Guid messageId)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.DeleteMessageAsync(conversationId, messageId, userId);
        if (!result.Succeeded)
        {
            await SendErrorAsync("delete_message_failed", result.Message);
            return;
        }

        await Clients.Group(GetConversationGroupName(conversationId)).SendAsync("MessageDeleted", new
        {
            ConversationId = conversationId,
            MessageId = messageId,
            DeletedByUserId = userId,
            DeletedAt = DateTime.UtcNow
        });
    }

    public async Task CreateDirectConversation(CreateDirectConversationRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.CreateDirectConversationAsync(userId, request);
        if (!result.Succeeded)
        {
            await SendErrorAsync("create_direct_conversation_failed", result.Message);
            return;
        }

        var conversation = (ConversationResponse)result.Value!;
        await JoinConversation(conversation.Id);
        await Clients.Caller.SendAsync("ConversationCreated", conversation);
    }

    public async Task CreateGroupConversation(CreateGroupConversationRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _conversationService.CreateGroupConversationAsync(userId, request);
        if (!result.Succeeded)
        {
            await SendErrorAsync("create_group_conversation_failed", result.Message);
            return;
        }

        var conversation = (ConversationResponse)result.Value!;
        await JoinConversation(conversation.Id);
        await Clients.Caller.SendAsync("ConversationCreated", conversation);
    }

    public async Task JoinRoom(MessageViewModel model, string? userId = null)
    {
        if (!TryGetConversationId(model.RoomName, out var conversationId))
        {
            await Clients.Caller.SendAsync("JoinRoomError", "RoomName must be a conversation id for the production chat hub.");
            return;
        }

        await JoinConversation(conversationId);
    }

    public async Task SendMessage(MessageViewModel model)
    {
        if (!TryGetConversationId(model.RoomName, out var conversationId))
        {
            await Clients.Caller.SendAsync("SendMessageError", "RoomName must be a conversation id for the production chat hub.");
            return;
        }

        await SendConversationMessage(conversationId, new SendMessageRequest
        {
            Content = model.Content ?? string.Empty
        });
    }

    public async Task LeaveRoom(RoomViewModel model, string? userId = null)
    {
        if (!TryGetConversationId(model.RoomName, out var conversationId))
        {
            await Clients.Caller.SendAsync("LeaveRoomError", "RoomName must be a conversation id for the production chat hub.");
            return;
        }

        await LeaveConversation(conversationId);
    }

    public async Task CreateRoom(RoomViewModel model)
    {
        await Clients.Caller.SendAsync("CreateRoomError", "Use CreateGroupConversation or POST api/conversations/groups for production conversations.");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        await _connectionTracker.AddConnectionAsync(userId, Context.ConnectionId);

        var conversationIds = await _conversationService.GetActiveConversationIdsAsync(userId);
        foreach (var conversationId in conversationIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationId));
        }

        _logger.LogInformation(
            "Authenticated client connected: {ConnectionId}, UserId: {UserId}, ConversationCount: {ConversationCount}",
            Context.ConnectionId,
            userId,
            conversationIds.Count);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        await _connectionTracker.RemoveConnectionAsync(userId, Context.ConnectionId);

        _logger.LogInformation(
            "Authenticated client disconnected: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId,
            userId);

        await base.OnDisconnectedAsync(exception);
    }

    private string GetCurrentUserId()
    {
        if (Context.User is null)
        {
            throw new HubException("Authenticated user is missing.");
        }

        try
        {
            return Context.User.GetRequiredUserId();
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    private async Task SendErrorAsync(string code, string message)
    {
        await Clients.Caller.SendAsync(RealtimeErrorEventName, new RealtimeErrorEvent
        {
            Code = code,
            Message = string.IsNullOrWhiteSpace(message) ? "The realtime operation failed." : message
        });
    }

    private static bool TryGetConversationId(string? value, out Guid conversationId)
    {
        return Guid.TryParse(value, out conversationId);
    }

    private static string GetConversationGroupName(Guid conversationId)
    {
        return $"conversation:{conversationId:N}";
    }
}
