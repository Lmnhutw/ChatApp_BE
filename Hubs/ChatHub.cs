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
using System.Collections.Generic;

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
            _logger.LogInformation("Received request to create room: {RoomName} by {CreatedBy}", model.RoomName, model.CreatedBy);

            try
            {
                // Log the incoming RoomViewModel
                _logger.LogInformation("RoomViewModel: {@RoomViewModel}", model);

                // Fetch the user who is creating the room
                var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == model.CreatedBy);

                // Check if the user exists and log the result
                if (user == null)
                {
                    _logger.LogWarning("User not found: {CreatedBy}", model.CreatedBy);
                    throw new ArgumentException("User not found");
                }

                // Log the found user details
                _logger.LogInformation("User: {@User}", user);

                /* var room = new Room
                {
                    Name = model.RoomName,
                    CreatedBy = user.UserName,  // Store username or any other identifier
                    CreatedAt = DateTime.UtcNow
                };

                // Add the room to the database context
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync(); // Save changes to persist the room

                // Now that the room is created, add the user as a member
                var roomUser = new RoomUser
                {
                    Id = user.Id, // This is the ID of the user joining the room
                    RoomId = room.RoomId,
                    IsMember = true // Mark this user as a member
                };

                // Associate the user with the room
                room.RoomUsers.Add(roomUser);
                await _context.SaveChangesAsync(); // Save changes to persist the association

                var roomViewModel = new RoomViewModel
                {
                    RoomId = room.RoomId,
                    RoomName = room.Name,
                    CreatedBy = room.CreatedBy,
                    Members = new List<RoleUserViewModel>() // Populate as needed
                }; */

                // Create the room and log the details
                var room = new Room
                {
                    Name = model.RoomName,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    RoomId = model.RoomId,
                };

                _logger.LogInformation("Room to be created: {@Room}", room);

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                var roomViewModel = new RoomViewModel
                {
                    RoomId = room.RoomId,
                    RoomName = room.Name,
                    CreatedBy = room.CreatedBy,
                    Members = new List<RoleUserViewModel>()
                };

                _logger.LogInformation("Room created successfully: {@RoomViewModel}", roomViewModel);

                await Clients.Caller.SendAsync("RoomCreated", roomViewModel);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("CreateRoomError", "Validation error: " + ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("CreateRoomError", "Database error: " + dbEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("CreateRoomError", "Unexpected error: " + ex.Message);
            }
        }

        public async Task JoinRoom(MessageViewModel model, string userId)
        {
            _logger.LogInformation("User {User} attempting to join room {RoomName}", model.FullName, model.RoomName);

            try
            {
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
                });

                await _context.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, model.RoomName);
                await Clients.Group(model.RoomName).SendAsync("UserJoined", user.FullName);

                _logger.LogInformation("User {User} successfully joined room {RoomName}", model.FullName, model.RoomName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while joining room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("JoinRoomError", "Validation error: " + ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while joining room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("JoinRoomError", "Database error: " + dbEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while joining room {RoomName}", model.RoomName);
                await Clients.Caller.SendAsync("JoinRoomError", "Unexpected error: " + ex.Message);
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