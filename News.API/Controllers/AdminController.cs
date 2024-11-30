using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<ApplicationUser> _userManager,IAccountService _accountService , INewsService _newsService ,IUserService _userService) : ControllerBase
    {
        // POST : api/admin/lock-user/{id}
        [HttpPost("lock-user/{id}")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var currentUser = await _userService.GetCurrentUserAsync();
            var isAdmin = _accountService.CheckAdminRoleAsync(currentUser);
            if (isAdmin.Result)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);
                return Ok(new { result = "User locked" });
            }
            else
                return Unauthorized();
        }
        // POST : api/admin/unlock-user/{id}
        [HttpPost("unlock-user/{id}")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var currentUser = await _userService.GetCurrentUserAsync();
            var isAdmin = _accountService.CheckAdminRoleAsync(currentUser);
            if(isAdmin.Result)
            {
                user.LockoutEnd = null;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return Ok(new { result = "User unlocked" });
                else
                    return BadRequest();
            }
            else
                return Unauthorized();
        }
        //POST : api/admin/add-category 
        [HttpPost("add-category")]
        public async Task<IActionResult> AddCategory([FromBody] AddOrUpdateCategoryDto categoryDto)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var isAdmin = await _accountService.CheckAdminRoleAsync(currentUser);
            if (isAdmin)
            {
                try
                {

                    var result = await _newsService.AddCategoryAsync(categoryDto);
                    if (!result)
                    {
                        return BadRequest("Failed to add category.");
                    }
                    return Ok("Category added successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
            }
            else
            {
                return Unauthorized();
            }
        }
        //DELETE : api/admin/delete-category
        [HttpDelete("delete-category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var isAdmin = await _accountService.CheckAdminRoleAsync(currentUser);
            if (isAdmin)
            {
                try
                {
                    var result = await _newsService.DeleteCategoryAsync(id);
                    if (!result)
                    {
                        return NotFound("Category not found.");
                    }
                    return Ok("Category deleted successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
            }
            return Unauthorized();
        }
        //PUT : api/admin/update-category
        [HttpPut("update-category/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] AddOrUpdateCategoryDto categoryDto)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var isAdmin = await _accountService.CheckAdminRoleAsync(currentUser);
            if (isAdmin)
            {
                try
                {
                    var result = await _newsService.UpdateCategoryAsync(id, categoryDto);
                    if (!result)
                    {
                        return NotFound("Category not found or update failed.");
                    }
                    return Ok("Category updated successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
