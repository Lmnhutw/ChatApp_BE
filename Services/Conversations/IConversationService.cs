using ChatApp_BE.ViewModels.ConversationViewModel;

namespace ChatApp_BE.Services.Conversations;

public interface IConversationService
{
    Task<IReadOnlyCollection<Guid>> GetActiveConversationIdsAsync(string userId);

    Task<ConversationServiceResult> GetConversationsAsync(string userId);

    Task<ConversationServiceResult> GetConversationAsync(Guid conversationId, string userId);

    Task<ConversationServiceResult> CreateDirectConversationAsync(string userId, CreateDirectConversationRequest request);

    Task<ConversationServiceResult> CreateGroupConversationAsync(string userId, CreateGroupConversationRequest request);

    Task<ConversationServiceResult> GetMembersAsync(Guid conversationId, string userId);

    Task<ConversationServiceResult> AddMemberAsync(Guid conversationId, string userId, AddConversationMemberRequest request);

    Task<ConversationServiceResult> RemoveMemberAsync(Guid conversationId, string userId, string memberUserId);

    Task<ConversationServiceResult> GetMessagesAsync(Guid conversationId, string userId, DateTime? before, int take);

    Task<ConversationServiceResult> SendMessageAsync(Guid conversationId, string userId, SendMessageRequest request);

    Task<ConversationServiceResult> UpdateMessageAsync(Guid conversationId, Guid messageId, string userId, UpdateMessageRequest request);

    Task<ConversationServiceResult> DeleteMessageAsync(Guid conversationId, Guid messageId, string userId);
}

public sealed class ConversationServiceResult
{
    private ConversationServiceResult(bool succeeded, ConversationServiceResultStatus status, string message, object? value = null)
    {
        Succeeded = succeeded;
        Status = status;
        Message = message;
        Value = value;
    }

    public bool Succeeded { get; }

    public ConversationServiceResultStatus Status { get; }

    public string Message { get; }

    public object? Value { get; }

    public static ConversationServiceResult Success(object value) =>
        new(true, ConversationServiceResultStatus.Success, string.Empty, value);

    public static ConversationServiceResult BadRequest(string message) =>
        new(false, ConversationServiceResultStatus.BadRequest, message);

    public static ConversationServiceResult Forbidden(string message = "") =>
        new(false, ConversationServiceResultStatus.Forbidden, message);

    public static ConversationServiceResult NotFound(string message) =>
        new(false, ConversationServiceResultStatus.NotFound, message);
}

public enum ConversationServiceResultStatus
{
    Success,
    BadRequest,
    Forbidden,
    NotFound
}
