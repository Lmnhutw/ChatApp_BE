using ChatApp_BE.ViewModels.Realtime;

namespace ChatApp_BE.Services.Realtime;

public interface IUserPresenceService
{
    Task<UserPresenceEvent> MarkOnlineAsync(string userId, int connectionCount);

    Task<UserPresenceEvent> MarkOfflineAsync(string userId);
}
