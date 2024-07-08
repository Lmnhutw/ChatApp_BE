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
            try
            {
                var msg = new Message
                {
                    Content = model.Content,
                    Id = model.UserId,
                    RoomId = model.RoomId,
                    Timestamp = DateTime.UtcNow
                };

                await _messageService.SaveMessageAsync(msg);
                await Clients.Group(model.RoomId.ToString()).SendAsync("ReceiveMessage", model.FullName, model.Content, msg.Timestamp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
                throw;
            }
        }

        public async Task JoinRoom(int roomId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("ShowWho", $"{Context.ConnectionId} joined the room.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinRoom: {ex.Message}");
                throw;
            }
        }

        public async Task LeaveRoom(int roomId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("ShowWho", $"{Context.ConnectionId} left the room.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LeaveRoom: {ex.Message}");
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("Client connected: " + Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OnConnectedAsync: " + ex.Message);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            { 
                Console.WriteLine("Client disconnected: " + Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OnDisconnectedAsync: " + ex.Message);
                throw;
            }
        }
    }
}
    