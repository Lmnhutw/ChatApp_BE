using ChatApp_BE.Data;
using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Services
{
    public class MessageService
    {
        private readonly ChatAppContext _ChatAppContext;

        public MessageService(ChatAppContext ChatAppCxt)
        {
            _ChatAppContext = ChatAppCxt;
        }
    }
}