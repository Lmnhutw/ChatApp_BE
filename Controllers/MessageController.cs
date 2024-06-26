using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatApp_BE.Data;

namespace ChatApp_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ChatAppContext _cxt;

        public MessageController(ChatAppContext cxt)
        {
            _cxt = cxt;
        }

        // GET: api/Message
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageViewModel>>> GetMessages()
        {
            var messages = await _cxt.Messages
                                         .Include(m => m.User)
                                         .Include(m => m.Room)
                                         .ToListAsync();

            var messageViewModels = new List<MessageViewModel>();
            foreach (var message in messages)
            {
                messageViewModels.Add(new MessageViewModel
                {
                    MessageId = message.MessageId,
                    Content = message.Content,
                    Timestamp = message.Timestamp,
                    UserId = message.Id,
                    UserName = message.User.FullName,
                    RoomId = message.RoomId,
                    RoomName = message.Room.Name
                });
            }

            return Ok(messageViewModels);
        }

        // GET: api/Message/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageViewModel>> GetMessage(int id)
        {
            var message = await _cxt.Messages
                                        .Include(m => m.User)
                                        .Include(m => m.Room)
                                        .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null)
            {
                return NotFound();
            }

            var messageViewModel = new MessageViewModel
            {
                MessageId = message.MessageId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                UserId = message.Id,
                UserName = message.User.FullName,
                RoomId = message.RoomId,
                RoomName = message.Room.Name
            };

            return Ok(messageViewModel);
        }

        // POST: api/Message
        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(MessageViewModel messageViewModel)
        {
            var message = new Message
            {
                Content = messageViewModel.Content,
                Timestamp = messageViewModel.Timestamp,
                RoomId = messageViewModel.RoomId,
                Id = messageViewModel.UserId
            };

            _cxt.Messages.Add(message);
            await _cxt.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.MessageId }, message);
        }

        // PUT: api/Message/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, MessageViewModel messageViewModel)
        {
            if (id != messageViewModel.MessageId)
            {
                return BadRequest();
            }

            var message = await _cxt.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Content = messageViewModel.Content;
            message.Timestamp = messageViewModel.Timestamp;
            message.RoomId = messageViewModel.RoomId;
            message.Id = messageViewModel.UserId;

            _cxt.Entry(message).State = EntityState.Modified;

            try
            {
                await _cxt.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Message/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _cxt.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _cxt.Messages.Remove(message);
            await _cxt.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(int id)
        {
            return _cxt.Messages.Any(e => e.MessageId == id);
        }
    }
}