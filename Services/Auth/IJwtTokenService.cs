using ChatApp_BE.Models;

namespace ChatApp_BE.Services.Auth;

public interface IJwtTokenService
{
    Task<string> CreateJwtAsync(ApplicationUser user);
}
