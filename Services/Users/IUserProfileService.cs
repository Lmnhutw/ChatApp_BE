using ChatApp_BE.ViewModels.ProfileViewModel;

namespace ChatApp_BE.Services.Users;

public interface IUserProfileService
{
    Task<UserServiceResult> GetProfileAsync(string userId);

    Task<UserServiceResult> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    Task<UserServiceResult> SearchUsersAsync(string userId, string query, int take);

    Task<UserServiceResult> GetBlockedUsersAsync(string userId);

    Task<UserServiceResult> BlockUserAsync(string userId, BlockUserRequest request);

    Task<UserServiceResult> UnblockUserAsync(string userId, string blockedUserId);
}

public sealed class UserServiceResult
{
    private UserServiceResult(bool succeeded, UserServiceResultStatus status, string message, object? value = null)
    {
        Succeeded = succeeded;
        Status = status;
        Message = message;
        Value = value;
    }

    public bool Succeeded { get; }

    public UserServiceResultStatus Status { get; }

    public string Message { get; }

    public object? Value { get; }

    public static UserServiceResult Success(object value) =>
        new(true, UserServiceResultStatus.Success, string.Empty, value);

    public static UserServiceResult BadRequest(string message) =>
        new(false, UserServiceResultStatus.BadRequest, message);

    public static UserServiceResult NotFound(string message) =>
        new(false, UserServiceResultStatus.NotFound, message);
}

public enum UserServiceResultStatus
{
    Success,
    BadRequest,
    NotFound
}
