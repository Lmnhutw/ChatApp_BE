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
using SendGrid.Helpers.Mail.Model;

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
        public async Task<IActionResult> Register(RegisterViewModel model, [FromServices] ILogger<authController> logger)
        {
            logger.LogInformation("Starting registration for email: {Email}", model.Email);

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    logger.LogWarning("Email already used: {Email}", model.Email);
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
                    if (code == null)
                    {
                        logger.LogError("Failed to generate email confirmation token for user: {UserId}", user.Id);
                        return StatusCode(500, "Failed to generate email confirmation token.");
                    }

                    logger.LogInformation("Generated email confirmation token for user: {UserId}", user.Id);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    if (code == null)
                    {
                        logger.LogError("Failed to encode email confirmation token for user: {UserId}", user.Id);
                        return StatusCode(500, "Failed to encode email confirmation token.");
                    }

                    user.EmailConfirmationToken = code;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        logger.LogError("Failed to save email confirmation token for user: {UserId}", user.Id);
                        return StatusCode(500, "Failed to save email confirmation token.");
                    }

                    logger.LogInformation("Encoded email confirmation token for user: {UserId}", user.Id);

                    var confirmationLink = Url.ActionLink(
                        nameof(ConfirmEmail), "auth",
                        new { userId = user.Id, token = code },
                        Request.Scheme
                    );

                    if (confirmationLink == null)
                    {
                        logger.LogError("Failed to generate confirmation link for user: {UserId}", user.Id);
                        return StatusCode(500, "Failed to generate confirmation link.");
                    }

                    logger.LogInformation("Generated confirmation link for user: {UserId}", user.Id);

                    try
                    {
                        await _emailSender.SendEmailAsync(
                            model.Email,
                            "Confirm your email",
                            $"Please confirm your email by clicking <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>here</a>."
                        );
                        logger.LogInformation("Sent email confirmation link to: {Email}", model.Email);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send email to: {Email}", model.Email);
                        return StatusCode(500, "Failed to send email.");
                    }

                    return Ok(new { Message = "Registration successful! Please check your email to confirm your account." });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    logger.LogError("Registration error: {ErrorDescription}", error.Description);
                    return BadRequest($"{error.Description}");
                }
            }

            logger.LogWarning("Invalid model state for email: {Email}", model.Email);
            return BadRequest(ModelState);
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

                        //var token = tokenHandler.CreateToken(tokenDescriptor);
                        //var tokenString = tokenHandler.WriteToken(token);

                        //return Ok(new { Token = tokenString });
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