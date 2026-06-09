using ChatApp_BE.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Services.Auth;

public interface IEmailConfirmationService
{
    Task SendConfirmationEmailAsync(ApplicationUser user, string email, IUrlHelper urlHelper, string requestScheme);
}
