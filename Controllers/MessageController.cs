using ChatApp_BE.Data;
using ChatApp_BE.Extensions;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ChatAppContext _context;

    public MessageController(ChatAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageViewModel>>> GetMessages()
    {
        var messages = await _context.Messages
            .Include(message => message.User)
            .Include(message => message.Room)
            .ToListAsync();

        var messageViewModels = messages.Select(message => new MessageViewModel
        {
            MessageId = message.MessageId,
            Content = message.Content,
            Timestamp = message.Timestamp,
            UserId = message.User?.Id ?? string.Empty,
            DisplayName = message.User?.FullName ?? string.Empty,
            RoomId = message.RoomId,
            RoomName = message.Room.Name ?? string.Empty
        }).ToList();

        return Ok(messageViewModels);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MessageViewModel>> GetMessage(int id)
    {
        var message = await _context.Messages
            .Include(message => message.User)
            .Include(message => message.Room)
            .FirstOrDefaultAsync(message => message.MessageId == id);

        if (message is null)
        {
            return NotFound();
        }

        var messageViewModel = new MessageViewModel
        {
            MessageId = message.MessageId,
            Content = message.Content,
            Timestamp = message.Timestamp,
            UserId = message.User?.Id ?? string.Empty,
            DisplayName = message.User?.FullName ?? string.Empty,
            RoomId = message.RoomId,
            RoomName = message.Room.Name ?? string.Empty
        };

        return Ok(messageViewModel);
    }

    [HttpPost]
    public async Task<ActionResult<MessageViewModel>> PostMessage(MessageViewModel messageViewModel)
    {
        var message = new Message
        {
            Content = messageViewModel.Content ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            RoomId = messageViewModel.RoomId,
            Id = User.GetRequiredUserId(),
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        messageViewModel.MessageId = message.MessageId;
        messageViewModel.Timestamp = message.Timestamp;
        messageViewModel.UserId = message.Id;

        return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, messageViewModel);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutMessage(int id, MessageViewModel messageViewModel)
    {
        if (id != messageViewModel.MessageId)
        {
            return BadRequest();
        }

        var currentUserId = User.GetRequiredUserId();
        var message = await _context.Messages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        if (message.Id != currentUserId)
        {
            return Forbid();
        }

        message.Content = messageViewModel.Content ?? string.Empty;
        message.Timestamp = DateTime.UtcNow;
        message.RoomId = messageViewModel.RoomId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MessageExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    private bool MessageExists(int id)
    {
        return _context.Messages.Any(message => message.MessageId == id);
    }
}
