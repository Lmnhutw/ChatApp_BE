using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ChatApp_BE.Services;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;
using System;
using SendGrid.Helpers.Mail;

namespace ChatApp_BE.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;
        private readonly RoomService _roomService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(MessageService messageService, RoomService roomService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task JoinRoom(MessageViewModel model)
        {
            try
            {
                _logger.LogInformation("User {User} attempting to join room {RoomName}", model.FullName, model.RoomName);

                // Validate inputs
                if (string.IsNullOrEmpty(model.RoomName) || string.IsNullOrEmpty(model.FullName))
                {
                    _logger.LogWarning("Invalid join room request: roomName = {RoomName}, FullName = {FullName}", model.RoomName, model.FullName);
                    throw new ArgumentException("Invalid RoomName or FullName");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, model.RoomName.ToString());
                await Clients.Group(model.RoomName.ToString()).SendAsync("UserJoined", model.FullName);

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
                _logger.LogInformation("User {User} attempting to send messages {Content}", model.FullName, model.Content);

                var chatMessage = new
                {
                    sender = model.FullName,
                    content = model.Content,
                    timeStamp = model.Timestamp.ToString("hh:mm:tt:zzz")
                };
                if (chatMessage is null)
                {
                    _logger.LogWarning("Messages can not be blank. Please type in!!!");
                    throw new ArgumentException("Invalid Messages");
                }

                _logger.LogInformation("Sending message from {User} to room {Room}: {Message}", model.FullName, model.RoomName, model.Content);
                await Clients.Group(model.RoomName).SendAsync("ReceiveMessage", chatMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage from user {User} to room {Room}", model.FullName, model.RoomName);
                throw;
            }
        }

        public async Task JoinSpecificChatRoom(MessageViewModel model)
        {
            try
            {
                _logger.LogInformation("User {User} joining specific chat room {RoomName}", model.FullName, model.RoomName);
                await Groups.AddToGroupAsync(Context.ConnectionId, model.RoomName);
                await Clients.Group(model.RoomName).SendAsync("ShowWho", $"{Context.ConnectionId} joined the room.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinSpecificChatRoom for user {User} and room {RoomName}", model.FullName, model.RoomName);
                throw;
            }
        }

        //public async Task LeaveRoom(RoomViewModel model)
        //{
        //    try
        //    {
        //        _logger.LogInformation("User {UserId} leaving room {RoomId}", Context.ConnectionId, model.RoomId);
        //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, model.RoomId.ToString());
        //        await Clients.Group(model.RoomId.ToString()).SendAsync("ShowWho", $"{Context.ConnectionId} left the room.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in LeaveRoom for user {UserId} and room {RoomId}", Context.ConnectionId, model.RoomId);
        //        throw;
        //    }
        //}

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