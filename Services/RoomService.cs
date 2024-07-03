using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using ChatApp_BE.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ChatApp_BE.Services
{
    public class RoomService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ChatAppContext _context;

        public RoomService(UserManager<ApplicationUser> userManager, ChatAppContext context)
        {
            _userManager = userManager;
            _context = context;
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
    }
}