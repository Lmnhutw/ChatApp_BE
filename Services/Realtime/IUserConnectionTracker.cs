namespace ChatApp_BE.Services.Realtime;

public interface IUserConnectionTracker
{
    Task AddConnectionAsync(string userId, string connectionId);

    Task RemoveConnectionAsync(string userId, string connectionId);

    Task<IReadOnlyCollection<string>> GetConnectionsAsync(string userId);

    Task<IReadOnlyCollection<string>> GetOnlineUserIdsAsync();
}
