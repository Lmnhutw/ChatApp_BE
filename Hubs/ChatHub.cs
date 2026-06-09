using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp_BE.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly ChatAppContext _context;

    public ChatHub(ILogger<ChatHub> logger, ChatAppContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task CreateRoom(RoomViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (string.IsNullOrWhiteSpace(model.RoomName))
        {
            await Clients.Caller.SendAsync("CreateRoomError", "Room name is required.");
            return;
        }

        var displayName = GetDisplayName(user);
        var room = new Room
        {
            Name = model.RoomName,
            CreatedBy = displayName,
            CreatedAt = DateTime.UtcNow,
            Id = user.Id
        };

        room.RoomUsers.Add(new RoomUser
        {
            RoomId = room.RoomId,
            Id = user.Id,
            IsMember = true,
            FullName = displayName
        });

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Name);

        await Clients.Caller.SendAsync("RoomCreated", new RoomViewModel
        {
            RoomId = room.RoomId,
            RoomName = room.Name,
            CreatedBy = room.CreatedBy,
            UserId = user.Id,
            Members = new List<RoleUserViewModel>()
        });
    }

    public async Task JoinRoom(MessageViewModel model, string? userId = null)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!string.IsNullOrWhiteSpace(userId) && userId != currentUser.Id)
        {
            _logger.LogWarning("Client supplied mismatched SignalR user id {SuppliedUserId} for authenticated user {UserId}", userId, currentUser.Id);
        }

        var room = await _context.Rooms
            .Include(room => room.RoomUsers)
            .FirstOrDefaultAsync(room => room.Name == model.RoomName);

        if (room is null)
        {
            await Clients.Caller.SendAsync("JoinRoomError", "Room does not exist.");
            return;
        }

        var displayName = GetDisplayName(currentUser);
        var member = room.RoomUsers.FirstOrDefault(roomUser => roomUser.Id == currentUser.Id);
        if (member is null)
        {
            room.RoomUsers.Add(new RoomUser
            {
                RoomId = room.RoomId,
                Id = currentUser.Id,
                IsMember = true,
                FullName = displayName
            });

            await _context.SaveChangesAsync();
        }
        else if (!member.IsMember)
        {
            member.IsMember = true;
            member.FullName = displayName;
            await _context.SaveChangesAsync();
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Name ?? string.Empty);
        await Clients.Group(room.Name ?? string.Empty).SendAsync("UserJoined", displayName);
    }

    public async Task SendMessage(MessageViewModel model)
    {
        var currentUser = await GetCurrentUserAsync();
        var room = await _context.Rooms.FirstOrDefaultAsync(room => room.Name == model.RoomName);
        if (room is null)
        {
            await Clients.Caller.SendAsync("SendMessageError", "Room does not exist.");
            return;
        }

        var isMember = await _context.RoomUsers.AnyAsync(roomUser =>
            roomUser.RoomId == room.RoomId &&
            roomUser.Id == currentUser.Id &&
            roomUser.IsMember);

        if (!isMember)
        {
            await Clients.Caller.SendAsync("SendMessageError", "You must join the room before sending messages.");
            return;
        }

        var messageEntity = new Message
        {
            Content = model.Content ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            RoomId = room.RoomId,
            Id = currentUser.Id
        };

        _context.Messages.Add(messageEntity);
        await _context.SaveChangesAsync();

        await Clients.Group(room.Name ?? string.Empty).SendAsync("ReceiveMessage", new
        {
            sender = GetDisplayName(currentUser),
            content = messageEntity.Content,
            timeStamp = messageEntity.Timestamp.ToString("hh:mm tt")
        });
    }

    public async Task LeaveRoom(RoomViewModel model, string? userId = null)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!string.IsNullOrWhiteSpace(userId) && userId != currentUser.Id)
        {
            _logger.LogWarning("Client supplied mismatched SignalR user id {SuppliedUserId} for authenticated user {UserId}", userId, currentUser.Id);
        }

        var room = await _context.Rooms
            .Include(room => room.RoomUsers)
            .FirstOrDefaultAsync(room => room.Name == model.RoomName);

        if (room is null)
        {
            await Clients.Caller.SendAsync("LeaveRoomError", "Room does not exist.");
            return;
        }

        var roomUser = room.RoomUsers.FirstOrDefault(member => member.Id == currentUser.Id);
        if (roomUser is not null)
        {
            room.RoomUsers.Remove(roomUser);
            await _context.SaveChangesAsync();
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Name ?? string.Empty);
        await Clients.Group(room.Name ?? string.Empty).SendAsync("UserLeft", GetDisplayName(currentUser));
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Authenticated client connected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, GetCurrentUserId());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Authenticated client disconnected: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, GetCurrentUserId());
        await base.OnDisconnectedAsync(exception);
    }

    private string GetCurrentUserId()
    {
        var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Authenticated user id claim is missing.");
        }

        return userId;
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var user = await _context.Users.FindAsync(GetCurrentUserId());
        return user ?? throw new HubException("Authenticated user no longer exists.");
    }

    private static string GetDisplayName(ApplicationUser user)
    {
        return user.FullName ?? user.DisplayName ?? user.UserName ?? user.Email ?? user.Id;
    }
}
