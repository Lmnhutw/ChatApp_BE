using ChatApp_BE.ViewModels.AuthViewModel;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterViewModel model, IUrlHelper urlHelper, string requestScheme);

    Task<AuthResult> ResendVerificationEmailAsync(string email, IUrlHelper urlHelper, string requestScheme);

    Task<AuthResult> ConfirmEmailAsync(string userId, string token);

    Task<AuthResult> LoginAsync(LoginViewModel model);

    Task<AuthResult> GetUserByEmailAsync(string email, string currentUserId);

    Task<AuthResult> GetUserByIdAsync(string userId, string currentUserId);

    Task<AuthResult> GetCurrentUserAsync(string currentUserId);

    Task<AuthResult> LogoutAsync(string currentUserId);
}

public sealed class AuthResult
{
    private AuthResult(
        bool succeeded,
        AuthResultStatus status,
        string message,
        object? value = null,
        IDictionary<string, string[]>? validationErrors = null)
    {
        Succeeded = succeeded;
        Status = status;
        Message = message;
        Value = value;
        ValidationErrors = validationErrors;
    }

    public bool Succeeded { get; }

    public AuthResultStatus Status { get; }

    public string Message { get; }

    public object? Value { get; }

    public IDictionary<string, string[]>? ValidationErrors { get; }

    public static AuthResult Success(object value, string message = "") =>
        new(true, AuthResultStatus.Success, message, value);

    public static AuthResult BadRequest(string message) =>
        new(false, AuthResultStatus.BadRequest, message);

    public static AuthResult Unauthorized(string message) =>
        new(false, AuthResultStatus.Unauthorized, message);

    public static AuthResult Forbidden() =>
        new(false, AuthResultStatus.Forbidden, string.Empty);

    public static AuthResult NotFound(string message) =>
        new(false, AuthResultStatus.NotFound, message);

    public static AuthResult Validation(IDictionary<string, string[]> validationErrors) =>
        new(false, AuthResultStatus.Validation, string.Empty, validationErrors: validationErrors);
}

public enum AuthResultStatus
{
    Success,
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
    Validation
}
