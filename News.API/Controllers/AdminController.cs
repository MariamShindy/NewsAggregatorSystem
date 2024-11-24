using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Entities;
using System.Security.Claims;

namespace News.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<ApplicationUser> _userManager, IUserService _userService) : ControllerBase
    {
        // POST : api/admin/lock-user/{id}
        [HttpPost("lock-user/{id}")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();
            var currentUser = await _userService.GetCurrentUser(currentUserId);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains("Admin"))
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);
                Console.WriteLine("Now lockedd");
                return Ok(new { result = "User locked" });
            }
            else
            {
                return Unauthorized();
            }
        }
        // POST : api/admin/unlock-user/{id}
        [HttpPost("unlock-user/{id}")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();
            var currentUser = await _userService.GetCurrentUser(currentUserId);
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains("Admin"))
            {
                user.LockoutEnd = null;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return Ok(new { result = "User unlocked" });
                else
                    return BadRequest();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
