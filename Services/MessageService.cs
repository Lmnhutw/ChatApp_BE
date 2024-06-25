using ChatApp_BE.Data;
using ChatApp_BE.Models;
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
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}