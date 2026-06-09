using ChatApp_BE.Helpers;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ChatApp_BE.Services.Auth;

public sealed class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSenders _emailSender;

    public EmailConfirmationService(
        UserManager<ApplicationUser> userManager,
        IEmailSenders emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task SendConfirmationEmailAsync(ApplicationUser user, string email, IUrlHelper urlHelper, string requestScheme)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var confirmationLink = urlHelper.ActionLink(
            action: "ConfirmEmail",
            controller: "auth",
            values: new { userId = user.Id, token = code },
            protocol: requestScheme);

        if (confirmationLink is null)
        {
            throw new InvalidOperationException("Unable to generate email confirmation link.");
        }

        var emailContent = await _emailSender.GetEmailTemplate(user.FullName ?? user.Email ?? email, confirmationLink);
        await _emailSender.SendEmailAsync("Confirm your email", email, emailContent);
    }
}
