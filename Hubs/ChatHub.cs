using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChatApp_BE.Services;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;
using System;

namespace ChatApp_BE.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;
        private readonly RoomService _roomService;

        public ChatHub(MessageService messageService, RoomService roomService)
        {
            _messageService = messageService;
            _roomService = roomService;
        }

        public async Task SendMessage(MessageViewModel model)
        {
            var msg = new Message
            {
                Content = model.Content,
                Id = model.UserId,
                RoomId = model.RoomId,
                Timestamp = DateTime.UtcNow
            };

            // Save message to the database
            await _messageService.SaveMessageAsync(msg);

            // Send message to all clients in the room
            await Clients.Group(model.RoomId.ToString()).SendAsync("ReceiveMessage", model.FullName, model.Content, msg.Timestamp);
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("ShowWho", $"{Context.ConnectionId} joined the room.");
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("ShowWho", $"{Context.ConnectionId} left the room.");
        }
    }
}