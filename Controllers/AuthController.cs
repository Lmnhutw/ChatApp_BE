using ChatApp_BE.Extensions;
using ChatApp_BE.Services.Auth;
using ChatApp_BE.ViewModels.AuthViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var result = await _authService.RegisterAsync(model, Url, Request.Scheme);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.Validation => BadRequest(new ValidationProblemDetails(result.ValidationErrors!)),
            AuthResultStatus.BadRequest => BadRequest(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [AllowAnonymous]
    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
    {
        var result = await _authService.ResendVerificationEmailAsync(request.Email, Url, Request.Scheme);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.BadRequest => BadRequest(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [AllowAnonymous]
    [HttpGet("resend-verification-email/{email}")]
    public async Task<IActionResult> ResendVerificationEmail(string email)
    {
        var result = await _authService.ResendVerificationEmailAsync(email, Url, Request.Scheme);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.BadRequest => BadRequest(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("GetUserEmail/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var result = await _authService.GetUserByEmailAsync(email, User.GetRequiredUserId());

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.NotFound => NotFound(new { result.Message }),
            AuthResultStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [AllowAnonymous]
    [HttpGet("confirmemail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.BadRequest => BadRequest(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _authService.LoginAsync(model);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.Unauthorized => Unauthorized(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("GetUserById/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var result = await _authService.GetUserByIdAsync(userId, User.GetRequiredUserId());

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.NotFound => NotFound(new { result.Message }),
            AuthResultStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var result = await _authService.GetCurrentUserAsync(User.GetRequiredUserId());

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            AuthResultStatus.Unauthorized => Unauthorized(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("check")]
    public IActionResult Check()
    {
        return Ok(new { Message = "User is authenticated", UserId = User.GetRequiredUserId() });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync(User.GetRequiredUserId());

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Value),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
