using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ChatApp_BE.Services
{
    public class MessageService
    {
        private readonly ChatAppContext _context;

        public MessageService(ChatAppContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetMessageHistoryAsync()
        {
            return await _context.Messages.ToListAsync();
        }
    }
}