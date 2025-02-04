using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Controllers
{
    [Authorize /*(Roles ="Admin")*/]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<ApplicationUser> _userManager,
        INewsService _newsService, IUserService _userService) : ControllerBase
    {
        // POST : api/admin/lock-user/{id}
        [HttpPost("lock-user/{id}")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
                return NotFound();
            user.LockoutEnd = DateTimeOffset.MaxValue;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { result = "User locked" });
            else
                return BadRequest();
        }
        // POST : api/admin/unlock-user/{id}
        [HttpPost("unlock-user/{id}")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
                return NotFound();
            user.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { result = "User unlocked" });
            else
                return BadRequest();
        }
        //POST : api/admin/add-category 
        [HttpPost("add-category")]
        public async Task<IActionResult> AddCategory([FromBody] AddOrUpdateCategoryDto categoryDto)
        {
                try
                {
                    var result = await _newsService.AddCategoryAsync(categoryDto);
                    if (!result)
                        return BadRequest("Failed to add category.");
                    return Ok("Category added successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
        }
        //DELETE : api/admin/delete-category
        [HttpDelete("delete-category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
                try
                {
                    var result = await _newsService.DeleteCategoryAsync(id);
                    if (!result)
                        return NotFound("Category not found.");
                    return Ok("Category deleted successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
        }
        //PUT : api/admin/update-category
        [HttpPut("update-category/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] AddOrUpdateCategoryDto categoryDto)
        { 
                try
                {
                    var result = await _newsService.UpdateCategoryAsync(id, categoryDto);
                    if (!result)
                        return NotFound("Category not found or update failed.");
                    return Ok("Category updated successfully.");
                }
                catch (Exception)
                {
                    return StatusCode(500, "Internal server error.");
                }
        }
        // GET : api/admin/all-users
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var userDtos = await _userService.GetAllUsersAsync();
                    return Ok(userDtos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while fetching users." });
            }
        }
    }
}
