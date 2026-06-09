using ChatApp_BE.Data;
using ChatApp_BE.Domain.Entities;
using ChatApp_BE.Domain.Enums;
using ChatApp_BE.ViewModels.Realtime;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Services.Realtime;

public sealed class UserPresenceService : IUserPresenceService
{
    private readonly ChatAppContext _context;

    public UserPresenceService(ChatAppContext context)
    {
        _context = context;
    }

    public async Task<UserPresenceEvent> MarkOnlineAsync(string userId, int connectionCount)
    {
        var now = DateTime.UtcNow;
        var presence = await _context.UserPresences.FirstOrDefaultAsync(item => item.UserId == userId);
        if (presence is null)
        {
            presence = new UserPresence
            {
                UserId = userId
            };
            _context.UserPresences.Add(presence);
        }

        presence.Status = UserPresenceStatus.Online;
        presence.ConnectionCount = connectionCount;
        presence.LastSeenAt = now;
        presence.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return ToEvent(presence);
    }

    public async Task<UserPresenceEvent> MarkOfflineAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var presence = await _context.UserPresences.FirstOrDefaultAsync(item => item.UserId == userId);
        if (presence is null)
        {
            presence = new UserPresence
            {
                UserId = userId
            };
            _context.UserPresences.Add(presence);
        }

        presence.Status = UserPresenceStatus.Offline;
        presence.ConnectionCount = 0;
        presence.LastSeenAt = now;
        presence.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return ToEvent(presence);
    }

    private static UserPresenceEvent ToEvent(UserPresence presence)
    {
        return new UserPresenceEvent
        {
            UserId = presence.UserId,
            Status = presence.Status,
            ConnectionCount = presence.ConnectionCount,
            LastSeenAt = presence.LastSeenAt
        };
    }
}
