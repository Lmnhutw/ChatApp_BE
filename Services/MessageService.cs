using Microsoft.Extensions.Logging;
using ChatApp_BE.Models;
using System.Threading.Tasks;
using ChatApp_BE.Data;

namespace ChatApp_BE.Services
{
    public class MessageService
    {
        private readonly ChatAppContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ILogger<MessageService> logger, ChatAppContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SaveMessageAsync(Message message)
        {
            try
            {
                _logger.LogInformation("Saving message: {Message}", message);
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message: {Message}", message);
                throw;
            }
        }
    }
}