using ChatApp_BE.Models;
using ChatApp_BE.ViewModels.AuthViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ChatApp_BE.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailConfirmationService emailConfirmationService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailConfirmationService = emailConfirmationService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(RegisterViewModel model, IUrlHelper urlHelper, string requestScheme)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            return AuthResult.BadRequest("Email was already used.");
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return AuthResult.Validation(ToValidationErrors(result));
        }

        await _emailConfirmationService.SendConfirmationEmailAsync(user, model.Email, urlHelper, requestScheme);

        return AuthResult.Success(new
        {
            Message = "Registration successful! Please check your Email to confirm your account.",
            model.Email
        });
    }

    public async Task<AuthResult> ResendVerificationEmailAsync(string email, IUrlHelper urlHelper, string requestScheme)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || await _userManager.IsEmailConfirmedAsync(user))
        {
            return AuthResult.BadRequest("Invalid email or email already confirmed.");
        }

        await _emailConfirmationService.SendConfirmationEmailAsync(user, email, urlHelper, requestScheme);

        return AuthResult.Success(new { Message = "Verification email resent! Please check your email to confirm your account." });
    }

    public async Task<AuthResult> ConfirmEmailAsync(string userId, string token)
    {
        _logger.LogInformation("Attempting to confirm email for user: {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return AuthResult.BadRequest("Invalid userId or token.");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email confirmation failed for user: {UserId}", user.Id);
            return AuthResult.BadRequest("Email confirmation failed.");
        }

        _logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
        return AuthResult.Success(new { Message = "Email confirmed successfully!" });
    }

    public async Task<AuthResult> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return AuthResult.Unauthorized("Invalid login attempt.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            model.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return AuthResult.Unauthorized("Account is temporarily locked. Please try again later.");
        }

        if (signInResult.IsNotAllowed)
        {
            return AuthResult.Unauthorized("Email confirmation is required before login.");
        }

        if (!signInResult.Succeeded)
        {
            return AuthResult.Unauthorized("Invalid login attempt.");
        }

        var tokenString = await _jwtTokenService.CreateJwtAsync(user);

        return AuthResult.Success(new
        {
            Token = tokenString,
            User = user.ToUserResponse()
        });
    }

    public async Task<AuthResult> GetUserByEmailAsync(string email, string currentUserId)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return AuthResult.NotFound("User not found.");
        }

        if (user.Id != currentUserId)
        {
            return AuthResult.Forbidden();
        }

        return AuthResult.Success(user.ToUserResponse());
    }

    public async Task<AuthResult> GetUserByIdAsync(string userId, string currentUserId)
    {
        if (userId != currentUserId)
        {
            return AuthResult.Forbidden();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthResult.NotFound("User not found.");
        }

        return AuthResult.Success(user.ToUserResponse());
    }

    public async Task<AuthResult> GetCurrentUserAsync(string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return AuthResult.Unauthorized("Authenticated user no longer exists.");
        }

        return AuthResult.Success(user.ToUserResponse());
    }

    public async Task<AuthResult> LogoutAsync(string currentUserId)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out", currentUserId);
        return AuthResult.Success(new { Message = "Logged out successfully!" });
    }

    private static Dictionary<string, string[]> ToValidationErrors(IdentityResult result)
    {
        return result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());
    }
}
