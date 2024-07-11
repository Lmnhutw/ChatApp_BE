using ChatApp_BE.Helpers;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels.AuthViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

                _logger.LogInformation(" response status code: {result}", result);
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

                    return Ok(new { Message = "Registration successful! Please check your Email to confirm your account.", Email = model.Email });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return BadRequest(ModelState);
        }

        //[HttpPost("send-test-email")]
        //public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        //{
        //    _logger.LogInformation("Received request to send test email to: {Email}", request.Email);

        //    try
        //    {
        //        await _emailSender.SendEmailAsync("Test Email", request.Email, request.Message);
        //        _logger.LogInformation("Test email sent successfully to: {Email}", request.Email);
        //        return Ok("Test email sent successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send test email to: {Email}", request.Email);
        //        return StatusCode(500, "Failed to send test email.");
        //    }
        //}

        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest("Invalid email or email already confirmed.");
            }

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
                email,
                emailContent
            );

            return Ok(new { Message = "Verification email resent! Please check your email to confirm your account." });
        }

        [HttpGet("GetUserEmail/{Email}")]
        public async Task<IActionResult> GetAllUser(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return BadRequest("Email not found.");
            }
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, [FromServices] ILogger<AuthController> logger)
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

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        var tokenString = tokenHandler.WriteToken(token);

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