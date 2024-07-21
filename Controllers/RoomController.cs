using ChatApp_BE.Data;
using ChatApp_BE.Models;
using ChatApp_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ChatApp_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ChatAppContext _context;
        private readonly RoomService _roomService;
        private readonly IConfiguration _config;
        private readonly ILogger<RoomController> _logger;

        public RoomController(
            ChatAppContext context,
            IConfiguration configuration,
            ILogger<RoomController> logger,
            RoomService roomService
            )
        {
            _context = context;
            _roomService = roomService;
            _config = configuration;
            _logger = logger;
        }

        [HttpGet("GetRoomList")]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Select(r => new RoomViewModel
                {
                    RoomId = r.RoomId,
                    RoomName = r.Name,
                    CreatedBy = r.CreatedBy,
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpDelete("DeleteRoom/{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            try
            {
                var room = await _context.Rooms.Include(r => r.RoomUsers).FirstOrDefaultAsync(r => r.RoomId == roomId);
                if (room == null)
                {
                    _logger.LogWarning("Room not found: {RoomId}", roomId);
                    return NotFound("Room not found");
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {RoomId} deleted", roomId);
                return Ok("Room deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}