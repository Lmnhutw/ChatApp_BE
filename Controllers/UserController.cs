using ChatApp_BE.Models;
using ChatApp_BE.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using ChatApp_BE.ViewModels.AuthViewModel;

using ChatApp_BE.ViewModels.Tests;
using SendGrid.Helpers.Mail.Model;
using System.Net.Http;
using Microsoft.Extensions.Configuration;


namespace ChatApp_BE.Controllers

{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class authController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSenders _emailSender;
        private readonly ILogger<authController> _logger;
        private readonly IConfiguration _config;

        public authController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           ILogger<authController> logger,
           IEmailSenders emailSender,
           IConfiguration configuration
           )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _config = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    return BadRequest("Email was already used.");
                }

                user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var confirmationLink = Url.ActionLink(
                        nameof(ConfirmEmail), "auth",
                        new { userId = user.Id, token = code },
                        Request.Scheme
                    );

                    var emailContent = await _emailSender.GetEmailTemplate(user.FullName, confirmationLink);
                    await _emailSender.SendEmailAsync(
                        "Confirm your email",
                        model.Email,
                        emailContent
                    );

                    return Ok(new { Message = "Registration successful! Please check your email to confirm your account." });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            _logger.LogInformation("Received request to send test email to: {Email}", request.Email);

            try
            {
                await _emailSender.SendEmailAsync("Test Email", request.Email, request.Message);
                _logger.LogInformation("Test email sent successfully to: {Email}", request.Email);
                return Ok("Test email sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to: {Email}", request.Email);
                return StatusCode(500, "Failed to send test email.");
            }
        }


        

        [AllowAnonymous]
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, [FromServices] ILogger<authController> logger)
        {
            logger.LogInformation("Attempting to confirm email for user: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User not found: {UserId}", userId);
                return BadRequest("Invalid userId or token.");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            logger.LogInformation("Decoded token for user: {UserId}", userId);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                logger.LogInformation("Email confirmed successfully for user: {UserId}", user.Id);
                return Ok("Email confirmed successfully!");
            }
            else
            {
                logger.LogWarning("Email confirmation failed for user: {UserId}", user.Id);
                return BadRequest("Email confirmation failed.");
            }
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User log out");
            return Ok(new { Message = "Logged out successfully!" });
        }

        //    [HttpPost("forgotpassword")]
        //    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var user = await _userManager.FindByEmailAsync(model.Email);
        //            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //            {
        //                return BadRequest("The user either does not exist or is not confirmed.");
        //            }

        //            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //            var resetLink = Url.Action(
        //                nameof(ResetPassword),
        //                "ApplicationUser",
        //                new { token, email = user.Email },
        //                Request.Scheme
        //                );
        //            await _emailSender.SendEmailAsync("Reset Password",
        //                model.Email,
        //                $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>."
        //                );

        //            return Ok(new { message = "Please check your email to reset your password." });
        //        }

        //        return BadRequest(ModelState);
        //    }

        //    [HttpGet("resetpassword")]
        //    public IActionResult ResetPassword(string token, string email)
        //    {
        //        var model = new ResetPasswordViewModel { Token = token, Email = email };
        //        return Ok(model); // Return a view in real implementation
        //    }

        //    [HttpPost("resetpassword")]
        //    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var user = await _userManager.FindByEmailAsync(model.Email);
        //            if (user == null)
        //            {
        //                return BadRequest("Invalid password reset request.");
        //            }

        //            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        //            if (result.Succeeded)
        //            {
        //                return Ok("Password reset successful!");
        //            }

        //            foreach (var error in result.Errors)
        //            {
        //                ModelState.AddModelError(string.Empty, error.Description);
        //            }
        //        }

        //        return BadRequest(ModelState);
        //    }
    }
}