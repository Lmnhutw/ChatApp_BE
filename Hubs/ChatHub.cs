using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChatApp_BE.Services;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;
using Microsoft.AspNetCore.Identity;

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
                FullName = model.FullName,
                RoomId = model.RoomId
            };

            // Save message to the database
            await _messageService.SaveMessageAsync(msg);

            // Send message to all clients in the room
            await Clients.Group((model.RoomId).ToString()).SendAsync("ReceiveMessage", model.FullName, model.Content);
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

        public async Task AddUserToRoom(int roomId, int userId, string role)
        {
            await _roomService.AddUserToRoomAsync(roomId, userId, role);
            await Clients.Group(roomId.ToString()).SendAsync("UserAdded", userId, role);
        }

        public async Task RemoveUserFromRoom(int roomId, int userId)
        {
            await _roomService.RemoveUserFromRoomAsync(roomId, userId);
            await Clients.Group(roomId.ToString()).SendAsync("UserRemoved", userId);
        }

        public async Task SetUserRole(int roomId, int userId, string role)
        {
            await _roomService.SetUserRoleAsync(roomId, userId, role);
            await Clients.Group(roomId.ToString()).SendAsync("UserRoleUpdated", userId, role);
        }
    }
}