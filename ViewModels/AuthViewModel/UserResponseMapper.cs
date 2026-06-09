using ChatApp_BE.Models;

namespace ChatApp_BE.ViewModels.AuthViewModel;

public static class UserResponseMapper
{
    public static UserResponse ToUserResponse(this ApplicationUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            Avatar = user.Avatar
        };
    }
}
