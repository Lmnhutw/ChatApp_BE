using System.Collections.Concurrent;

namespace ChatApp_BE.Services.Realtime;

public sealed class InMemoryUserConnectionTracker : IUserConnectionTracker
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connectionsByUserId = new();

    public Task AddConnectionAsync(string userId, string connectionId)
    {
        var connections = _connectionsByUserId.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);
        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string userId, string connectionId)
    {
        if (_connectionsByUserId.TryGetValue(userId, out var connections))
        {
            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                _connectionsByUserId.TryRemove(userId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetConnectionsAsync(string userId)
    {
        if (!_connectionsByUserId.TryGetValue(userId, out var connections))
        {
            return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
        }

        return Task.FromResult<IReadOnlyCollection<string>>(connections.Keys.ToArray());
    }

    public Task<IReadOnlyCollection<string>> GetOnlineUserIdsAsync()
    {
        return Task.FromResult<IReadOnlyCollection<string>>(_connectionsByUserId.Keys.ToArray());
    }
}
