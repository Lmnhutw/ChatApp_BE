using ChatApp_BE.Data;
using ChatApp_BE.Domain.Entities;
using ChatApp_BE.Domain.Enums;
using ChatApp_BE.ViewModels.ConversationViewModel;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Services.Conversations;

public sealed class ConversationService : IConversationService
{
    private const int MaxMessagePageSize = 100;

    private readonly ChatAppContext _context;

    public ConversationService(ChatAppContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Guid>> GetActiveConversationIdsAsync(string userId)
    {
        return await _context.ConversationMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId && member.LeftAt == null && member.Conversation.DeletedAt == null)
            .Select(member => member.ConversationId)
            .ToListAsync();
    }

    public async Task<ConversationServiceResult> GetConversationsAsync(string userId)
    {
        var conversations = await _context.Conversations
            .AsNoTracking()
            .Where(conversation =>
                conversation.DeletedAt == null &&
                conversation.Members.Any(member => member.UserId == userId && member.LeftAt == null))
            .Include(conversation => conversation.Members.Where(member => member.LeftAt == null))
                .ThenInclude(member => member.User)
            .Include(conversation => conversation.Messages
                .Where(message => message.DeletedAt == null)
                .OrderByDescending(message => message.CreatedAt)
                .Take(1))
                .ThenInclude(message => message.Sender)
            .OrderByDescending(conversation => conversation.Messages
                .Where(message => message.DeletedAt == null)
                .Max(message => (DateTime?)message.CreatedAt) ?? conversation.CreatedAt)
            .ToListAsync();

        return ConversationServiceResult.Success(conversations.Select(ToConversationResponse).ToList());
    }

    public async Task<ConversationServiceResult> GetConversationAsync(Guid conversationId, string userId)
    {
        var conversation = await GetConversationForMemberAsync(conversationId, userId, asTracking: false);
        if (conversation is null)
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        return ConversationServiceResult.Success(ToConversationResponse(conversation));
    }

    public async Task<ConversationServiceResult> CreateDirectConversationAsync(string userId, CreateDirectConversationRequest request)
    {
        if (request.OtherUserId == userId)
        {
            return ConversationServiceResult.BadRequest("Cannot create a direct conversation with yourself.");
        }

        var usersExist = await _context.Users.CountAsync(user => user.Id == userId || user.Id == request.OtherUserId);
        if (usersExist != 2)
        {
            return ConversationServiceResult.NotFound("User not found.");
        }

        var existingConversation = await _context.Conversations
            .Include(conversation => conversation.Members)
                .ThenInclude(member => member.User)
            .Include(conversation => conversation.Messages
                .Where(message => message.DeletedAt == null)
                .OrderByDescending(message => message.CreatedAt)
                .Take(1))
                .ThenInclude(message => message.Sender)
            .FirstOrDefaultAsync(conversation =>
                conversation.Type == ConversationType.Direct &&
                conversation.DeletedAt == null &&
                conversation.Members.Count(member => member.LeftAt == null) == 2 &&
                conversation.Members.Any(member => member.UserId == userId && member.LeftAt == null) &&
                conversation.Members.Any(member => member.UserId == request.OtherUserId && member.LeftAt == null));

        if (existingConversation is not null)
        {
            return ConversationServiceResult.Success(ToConversationResponse(existingConversation));
        }

        var conversation = new Conversation
        {
            Type = ConversationType.Direct,
            CreatedByUserId = userId,
            Members =
            {
                new ConversationMember
                {
                    UserId = userId,
                    Role = ConversationMemberRole.Owner
                },
                new ConversationMember
                {
                    UserId = request.OtherUserId,
                    Role = ConversationMemberRole.Member
                }
            }
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return await GetConversationAsync(conversation.Id, userId);
    }

    public async Task<ConversationServiceResult> CreateGroupConversationAsync(string userId, CreateGroupConversationRequest request)
    {
        var memberIds = request.MemberUserIds
            .Where(memberId => !string.IsNullOrWhiteSpace(memberId))
            .Append(userId)
            .Distinct()
            .ToList();

        var existingUserCount = await _context.Users.CountAsync(user => memberIds.Contains(user.Id));
        if (existingUserCount != memberIds.Count)
        {
            return ConversationServiceResult.NotFound("One or more users were not found.");
        }

        var conversation = new Conversation
        {
            Type = ConversationType.Group,
            Title = request.Title,
            CreatedByUserId = userId
        };

        foreach (var memberId in memberIds)
        {
            conversation.Members.Add(new ConversationMember
            {
                UserId = memberId,
                Role = memberId == userId ? ConversationMemberRole.Owner : ConversationMemberRole.Member
            });
        }

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return await GetConversationAsync(conversation.Id, userId);
    }

    public async Task<ConversationServiceResult> GetMembersAsync(Guid conversationId, string userId)
    {
        var isMember = await IsActiveMemberAsync(conversationId, userId);
        if (!isMember)
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        var members = await _context.ConversationMembers
            .AsNoTracking()
            .Where(member => member.ConversationId == conversationId && member.LeftAt == null)
            .Include(member => member.User)
            .OrderBy(member => member.JoinedAt)
            .Select(member => ToMemberResponse(member))
            .ToListAsync();

        return ConversationServiceResult.Success(members);
    }

    public async Task<ConversationServiceResult> AddMemberAsync(Guid conversationId, string userId, AddConversationMemberRequest request)
    {
        var conversation = await _context.Conversations
            .Include(conversation => conversation.Members)
            .FirstOrDefaultAsync(conversation => conversation.Id == conversationId && conversation.DeletedAt == null);

        if (conversation is null || !conversation.Members.Any(member => member.UserId == userId && member.LeftAt == null))
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        if (conversation.Type != ConversationType.Group)
        {
            return ConversationServiceResult.BadRequest("Members can only be added to group conversations.");
        }

        if (!IsAdmin(conversation, userId))
        {
            return ConversationServiceResult.Forbidden();
        }

        var userExists = await _context.Users.AnyAsync(user => user.Id == request.UserId);
        if (!userExists)
        {
            return ConversationServiceResult.NotFound("User not found.");
        }

        var existingMember = conversation.Members.FirstOrDefault(member => member.UserId == request.UserId);
        if (existingMember is null)
        {
            conversation.Members.Add(new ConversationMember
            {
                UserId = request.UserId,
                Role = ConversationMemberRole.Member
            });
        }
        else
        {
            existingMember.LeftAt = null;
        }

        await _context.SaveChangesAsync();
        return await GetMembersAsync(conversationId, userId);
    }

    public async Task<ConversationServiceResult> RemoveMemberAsync(Guid conversationId, string userId, string memberUserId)
    {
        var conversation = await _context.Conversations
            .Include(conversation => conversation.Members)
            .FirstOrDefaultAsync(conversation => conversation.Id == conversationId && conversation.DeletedAt == null);

        if (conversation is null || !conversation.Members.Any(member => member.UserId == userId && member.LeftAt == null))
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        if (conversation.Type != ConversationType.Group)
        {
            return ConversationServiceResult.BadRequest("Members can only be removed from group conversations.");
        }

        if (memberUserId != userId && !IsAdmin(conversation, userId))
        {
            return ConversationServiceResult.Forbidden();
        }

        var member = conversation.Members.FirstOrDefault(member => member.UserId == memberUserId && member.LeftAt == null);
        if (member is null)
        {
            return ConversationServiceResult.NotFound("Member not found.");
        }

        member.LeftAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ConversationServiceResult.Success(new { Message = "Member removed." });
    }

    public async Task<ConversationServiceResult> GetMessagesAsync(Guid conversationId, string userId, DateTime? before, int take)
    {
        var isMember = await IsActiveMemberAsync(conversationId, userId);
        if (!isMember)
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        var pageSize = Math.Clamp(take, 1, MaxMessagePageSize);
        var query = _context.ChatMessages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(message => message.CreatedAt < before.Value);
        }

        var messages = await query
            .Include(message => message.Sender)
            .OrderByDescending(message => message.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        var orderedMessages = messages
            .OrderBy(message => message.CreatedAt)
            .Select(ToMessageResponse)
            .ToList();

        return ConversationServiceResult.Success(new PagedResponse<MessageResponse>
        {
            Items = orderedMessages,
            NextBefore = messages.Count == pageSize ? messages.Min(message => message.CreatedAt) : null
        });
    }

    public async Task<ConversationServiceResult> SendMessageAsync(Guid conversationId, string userId, SendMessageRequest request)
    {
        var isMember = await IsActiveMemberAsync(conversationId, userId);
        if (!isMember)
        {
            return ConversationServiceResult.NotFound("Conversation not found.");
        }

        if (request.ReplyToMessageId.HasValue)
        {
            var replyExists = await _context.ChatMessages.AnyAsync(message =>
                message.Id == request.ReplyToMessageId &&
                message.ConversationId == conversationId);

            if (!replyExists)
            {
                return ConversationServiceResult.BadRequest("Reply message was not found in this conversation.");
            }
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = request.Content,
            Type = request.Type,
            ReplyToMessageId = request.ReplyToMessageId
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var createdMessage = await _context.ChatMessages
            .AsNoTracking()
            .Include(chatMessage => chatMessage.Sender)
            .FirstAsync(chatMessage => chatMessage.Id == message.Id);

        return ConversationServiceResult.Success(ToMessageResponse(createdMessage));
    }

    public async Task<ConversationServiceResult> UpdateMessageAsync(Guid conversationId, Guid messageId, string userId, UpdateMessageRequest request)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(message =>
                message.Id == messageId &&
                message.ConversationId == conversationId &&
                message.DeletedAt == null);

        if (message is null)
        {
            return ConversationServiceResult.NotFound("Message not found.");
        }

        if (message.SenderId != userId)
        {
            return ConversationServiceResult.Forbidden();
        }

        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;
        message.EditedByUserId = userId;

        await _context.SaveChangesAsync();

        var updatedMessage = await _context.ChatMessages
            .AsNoTracking()
            .Include(chatMessage => chatMessage.Sender)
            .FirstAsync(chatMessage => chatMessage.Id == messageId);

        return ConversationServiceResult.Success(ToMessageResponse(updatedMessage));
    }

    public async Task<ConversationServiceResult> DeleteMessageAsync(Guid conversationId, Guid messageId, string userId)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(message =>
                message.Id == messageId &&
                message.ConversationId == conversationId &&
                message.DeletedAt == null);

        if (message is null)
        {
            return ConversationServiceResult.NotFound("Message not found.");
        }

        if (message.SenderId != userId)
        {
            return ConversationServiceResult.Forbidden();
        }

        message.DeletedAt = DateTime.UtcNow;
        message.DeletedByUserId = userId;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ConversationServiceResult.Success(new { Message = "Message deleted." });
    }

    private async Task<Conversation?> GetConversationForMemberAsync(Guid conversationId, string userId, bool asTracking)
    {
        var query = _context.Conversations
            .Where(conversation =>
                conversation.Id == conversationId &&
                conversation.DeletedAt == null &&
                conversation.Members.Any(member => member.UserId == userId && member.LeftAt == null))
            .Include(conversation => conversation.Members.Where(member => member.LeftAt == null))
                .ThenInclude(member => member.User)
            .Include(conversation => conversation.Messages
                .Where(message => message.DeletedAt == null)
                .OrderByDescending(message => message.CreatedAt)
                .Take(1))
                .ThenInclude(message => message.Sender);

        return asTracking
            ? await query.FirstOrDefaultAsync()
            : await query.AsNoTracking().FirstOrDefaultAsync();
    }

    private async Task<bool> IsActiveMemberAsync(Guid conversationId, string userId)
    {
        return await _context.ConversationMembers.AnyAsync(member =>
            member.ConversationId == conversationId &&
            member.UserId == userId &&
            member.LeftAt == null);
    }

    private static bool IsAdmin(Conversation conversation, string userId)
    {
        return conversation.Members.Any(member =>
            member.UserId == userId &&
            member.LeftAt == null &&
            (member.Role == ConversationMemberRole.Owner || member.Role == ConversationMemberRole.Admin));
    }

    private static ConversationResponse ToConversationResponse(Conversation conversation)
    {
        return new ConversationResponse
        {
            Id = conversation.Id,
            Type = conversation.Type,
            Title = conversation.Title,
            CreatedByUserId = conversation.CreatedByUserId,
            CreatedAt = conversation.CreatedAt,
            Members = conversation.Members.Select(ToMemberResponse).ToList(),
            LastMessage = conversation.Messages
                .OrderByDescending(message => message.CreatedAt)
                .Select(ToMessageResponse)
                .FirstOrDefault()
        };
    }

    private static ConversationMemberResponse ToMemberResponse(ConversationMember member)
    {
        return new ConversationMemberResponse
        {
            UserId = member.UserId,
            FullName = member.User.FullName ?? member.User.DisplayName,
            Avatar = member.User.Avatar,
            Role = member.Role,
            JoinedAt = member.JoinedAt,
            LeftAt = member.LeftAt
        };
    }

    private static MessageResponse ToMessageResponse(ChatMessage message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderFullName = message.Sender.FullName ?? message.Sender.DisplayName,
            Content = message.DeletedAt is null ? message.Content : string.Empty,
            Type = message.Type,
            ReplyToMessageId = message.ReplyToMessageId,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            DeletedAt = message.DeletedAt
        };
    }
}
