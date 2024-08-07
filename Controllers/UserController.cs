﻿using ChatApp_BE.Models;
using ChatApp_BE.ViewModels.ManageViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageUser : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageUser> _logger;

        public ManageUser(
            UserManager<ApplicationUser> userManager,
            ILogger<ManageUser> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> Delete(DeleteUserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("ApplicationUser not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok("User deleted successfully.");
            }
            else
            {
                return BadRequest("Failed to delete user.");
            }
        }
    }

    public class GetListUser : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GetListUser> _logger;

        public GetListUser(
            UserManager<ApplicationUser> userManager,
            ILogger<GetListUser> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
    }
}