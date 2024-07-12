using ChatApp_BE.Data;

namespace ChatApp_BE.Services
{
    public class UserService
    {
        private readonly ChatAppContext _ChatAppContext;

        public UserService(ChatAppContext ChatAppCxt)
        {
            _ChatAppContext = ChatAppCxt;
        }
    }
}