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

namespace ChatApp_BE.Controllers

{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSenders _emailSender;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           ILogger<AuthController> logger,
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
                    return Ok(new { Message = "Registration successful! Please check your email to confirm your account." });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return BadRequest("");
                }
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, string Email)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null && user.EmailConfirmationToken != token)
            {
                return BadRequest($"Invalid userId or token.");
            }

            // Confirm the email
            userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            user.EmailConfirmationToken = code;
            //await _userManager.UpdateAsync(user);
            var confirmationLink = Url.ActionLink(nameof(ConfirmEmail), "ApplicationUser",
            new
            {
                userId = user.Id,
                code = token
            },
            Request.Scheme
            );
            await _emailSender.SendEmailAsync("Confirm your email",
                Email,
                $"Please confirm your email by clicking <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>here</a>.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Clear the EmailConfirmationToken once confirmed
                user.EmailConfirmationToken = null;
                await _userManager.UpdateAsync(user);

                return Ok("Email confirmed successfully!");
            }

            return BadRequest("Email confirmation failed.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    //if (!user.EmailConfirmed)
                    //{
                    //    return BadRequest("Email is not confirmed.");
                    //}
                    if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        return Unauthorized("Invalid login attempt.");
                    }
                    else
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.UTF8.GetBytes(_config.GetSection("Jwt:SecretKey").Value!);
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                            }),
                            Expires = DateTime.UtcNow.AddDays(14),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                        };

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        var tokenString = tokenHandler.WriteToken(token);

                        return Ok(new { Token = tokenString });
                    }
                    return Ok("Login successful!");
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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