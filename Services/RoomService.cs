using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ChatApp_BE.Services
{
    public class RoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ChatAppContext _context;

        public RoomService(UserManager<ApplicationUser> userManager, ChatAppContext context, ILogger<RoomService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task AddUserToRoomAsync(string userId, RoomViewModel model)
        {
            try
            {
                _logger.LogInformation("Adding user {UserId} to room {RoomId}", userId, model.RoomId);
                // Add your logic to add the user to the room
                var user = await _userManager.FindByIdAsync(userId);

                var room = new Room
                {
                    Name = model.RoomName,
                };

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to room {RoomId}", userId, model.RoomId);
                throw;
            }
        }

        //public async Task RemoveUserFromRoomAsync(int roomId, string userId)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Removing user {UserId} from room {RoomId}", userId, roomId);
        //        // Add your logic to remove the user from the room
        //        await Task.CompletedTask;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error removing user {UserId} from room {RoomId}", userId, roomId);
        //        throw;
        //    }
        //}
    }
}

//public async Task CreateRoomAsync(RoomViewModel model, string userId)
//{
//    var user = await _userManager.FindByIdAsync(userId);

//    var room = new Room
//    {
//        Name = model.RoomName,
//        FullName = model.AdminName,
//    };

//    _context.Rooms.Add(room);
//    await _context.SaveChangesAsync();
//}