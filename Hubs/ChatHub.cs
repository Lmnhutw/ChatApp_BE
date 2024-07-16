using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ChatApp_BE.Data;
using ChatApp_BE.Services;
using SendGrid.Helpers.Mail;

namespace ChatApp_BE.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;
        private readonly RoomService _roomService;
        private readonly ILogger<ChatHub> _logger;
        private readonly ChatAppContext _context;

        public ChatHub(MessageService messageService, RoomService roomService, ILogger<ChatHub> logger, ChatAppContext context)
        {
            _messageService = messageService;
            _roomService = roomService;
            _logger = logger;
            _context = context;
        }

        public async Task CreateRoom(RoomViewModel model)
        {
            try
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.FullName == model.CreatedBy);
                if (adminUser == null)
                {
                    _logger.LogWarning("User not found: {CreatedBy}", model.CreatedBy);
                    throw new ArgumentException("User not found");
                }

                var room = new Room
                {
                    Name = model.RoomName,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    Admin = adminUser,
                    FullName = adminUser.FullName
                };

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                var roomViewModel = new RoomViewModel
                {
                    RoomId = room.RoomId,
                    RoomName = room.Name,
                    CreatedBy = room.CreatedBy,
                    AdminName = room.Admin.FullName,
                    Members = new List<RoleUserViewModel>()
                };

                await Clients.Caller.SendAsync("RoomCreated", roomViewModel);

                _logger.LogInformation("Room {RoomName} created by {CreatedBy}", model.RoomName, model.CreatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room {RoomName}", model.RoomName);
                throw;
            }
        }

        public async Task JoinRoom(MessageViewModel model, string userId)
        {
            try
            {
                _logger.LogInformation("User {User} attempting to join room {RoomName}", model.FullName, model.RoomName);

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Name == model.RoomName);
                if (room == null)
                {
                    _logger.LogWarning("Room {RoomName} does not exist", model.RoomName);
                    throw new ArgumentException("Room does not exist");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    throw new ArgumentException("User not found");
                }

                room.RoomUsers.Add(new RoomUser
                {
                    RoomId = room.RoomId,
                    Id = user.Id,
                    IsMember = true
                });

                await _context.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, model.RoomName);
                await Clients.Group(model.RoomName).SendAsync("UserJoined", user.FullName);

                _logger.LogInformation("User {User} successfully joined room {RoomName}", model.FullName, model.RoomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinRoom for user {User} and room {RoomName}", model.FullName, model.RoomName);
                throw;
            }
        }

        public async Task SendMessage(MessageViewModel model)
        {
            try
            {
                _logger.LogInformation("User {User} attempting to send message to room {RoomName}", model.FullName, model.RoomName);

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Name == model.RoomName);
                if (room == null)
                {
                    _logger.LogWarning("Room {RoomName} does not exist", model.RoomName);
                    throw new ArgumentException("Room does not exist");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", model.UserId);
                    throw new ArgumentException("User not found");
                }

                var messageEntity = new Message
                {
                    Content = model.Content,
                    Timestamp = DateTime.UtcNow,
                    RoomId = room.RoomId,
                    User = user
                };

                _context.Messages.Add(messageEntity);
                await _context.SaveChangesAsync();

                var chatMessage = new
                {
                    sender = model.FullName,
                    content = model.Content,
                    timeStamp = messageEntity.Timestamp.ToString("hh:mm tt")
                };

                await Clients.Group(model.RoomName).SendAsync("ReceiveMessage", chatMessage);

                _logger.LogInformation("Message sent from user {User} to room {RoomName}", model.FullName, model.RoomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage from user {User} to room {RoomName}", model.FullName, model.RoomName);
                throw;
            }
        }

        public async Task LeaveRoom(RoomViewModel model, string userId)
        {
            try
            {
                var room = await _context.Rooms.Include(r => r.RoomUsers).FirstOrDefaultAsync(r => r.Name == model.RoomName);
                if (room != null)
                {
                    var roomUser = room.RoomUsers.FirstOrDefault(ru => ru.Id == userId);
                    if (roomUser != null)
                    {
                        room.RoomUsers.Remove(roomUser);
                        await _context.SaveChangesAsync();

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, model.RoomName);

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                        if (user != null)
                        {
                            await Clients.Group(model.RoomName).SendAsync("UserLeft", user.FullName);
                        }

                        _logger.LogInformation("User {UserId} left room {RoomName}", userId, model.RoomName);
                    }
                }
                else
                {
                    _logger.LogWarning("Room {RoomName} does not exist", model.RoomName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveRoom for user {UserId} and room {RoomName}", userId, model.RoomName);
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }
    }
}