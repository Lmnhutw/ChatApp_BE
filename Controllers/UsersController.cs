using ChatApp_BE.Extensions;
using ChatApp_BE.Services.Users;
using ChatApp_BE.ViewModels.ProfileViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userProfileService.GetProfileAsync(User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var result = await _userProfileService.UpdateProfileAsync(User.GetRequiredUserId(), request);
        return ToActionResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int take = 20)
    {
        var result = await _userProfileService.SearchUsersAsync(User.GetRequiredUserId(), query, take);
        return ToActionResult(result);
    }

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var result = await _userProfileService.GetBlockedUsersAsync(User.GetRequiredUserId());
        return ToActionResult(result);
    }

    [HttpPost("blocks")]
    public async Task<IActionResult> BlockUser(BlockUserRequest request)
    {
        var result = await _userProfileService.BlockUserAsync(User.GetRequiredUserId(), request);
        return ToActionResult(result);
    }

    [HttpDelete("blocks/{blockedUserId}")]
    public async Task<IActionResult> UnblockUser(string blockedUserId)
    {
        var result = await _userProfileService.UnblockUserAsync(User.GetRequiredUserId(), blockedUserId);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(UserServiceResult result)
    {
        return result.Status switch
        {
            UserServiceResultStatus.Success => Ok(result.Value),
            UserServiceResultStatus.BadRequest => BadRequest(new { result.Message }),
            UserServiceResultStatus.NotFound => NotFound(new { result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
