using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Entities;

namespace News.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
	[ApiController]
	public class AdminController(UserManager<ApplicationUser> _userManager) : ControllerBase
	{
        // POST : api/admin/lock-user/{id}
        [HttpPost("lock-user/{id}")]
		public async Task<IActionResult> LockUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

			user.LockoutEnd = DateTimeOffset.MaxValue; 
			await _userManager.UpdateAsync(user);
			Console.WriteLine("Now lockedd");
			return Ok(new { result = "User locked" });
		}
        // POST : api/admin/unlock-user/{id}
        [HttpPost("unlock-user/{id}")]
		public async Task<IActionResult> UnlockUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			user.LockoutEnd = null;  
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
				return Ok(new { result = "User unlocked" });

			return BadRequest(result.Errors);
		}
	}
}
