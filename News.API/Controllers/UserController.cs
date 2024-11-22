using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Entities;
using System.Security.Claims;

namespace News.API.Controllers
{
    [Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class UserController( IUserService _userService,IFavoriteService _favoriteService , INewsService _newsService) : ControllerBase
	{

        //Choose categories end point 

        // GET: api/user/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = await _userService.GetCurrentUser(userId);
            if (user == null) 
                return BadRequest();

            var userInfo = new
            {
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.ProfilePicUrl
            };
            return Ok(userInfo);
        }

        // PUT: api/user/me
        [HttpPut("me")]
        public async Task<IActionResult> EditUserInfo([FromBody] EditUserModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.UpdateUser(userId, model);
            if (result.Succeeded)
                return Ok(new { result = "User updated successfully" });
            return BadRequest(result.Errors);
        }

        // POST: api/user/send-feedback
        [HttpPost("send-feedback")]
        [AllowAnonymous]
        public async Task<IActionResult> SendFeedbackAsync([FromBody] FeedbackModel feedbackModel)
        {
            if (feedbackModel == null ||
                string.IsNullOrWhiteSpace(feedbackModel.FullName) ||
                string.IsNullOrWhiteSpace(feedbackModel.Email) ||
                string.IsNullOrWhiteSpace(feedbackModel.Subject) ||
                string.IsNullOrWhiteSpace(feedbackModel.Message))
            {
                return BadRequest(new { Status = "Error", Message = "All fields are required." });
            }
            try
            {
                bool isSent = await _userService.SendFeedback(feedbackModel);
                return Ok(new { Status = "Success", Message = "Feedback received." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending feedback. {ex}");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while sending feedback. Please try again later." });
            }
        }
      
        // POST: api/user/favorites/newsId
        [HttpPost("favorites/{newsId}")]
        public async Task<IActionResult> AddToFavorites(string newsId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = await _userService.GetCurrentUser(userId);

            var articleExists = await _newsService.CheckArticleExists(newsId);
            if (!articleExists)
                return NotFound(new { message = "Article not found" });

            var alreadyFavorited = await _favoriteService.IsArticleFavorited(userId, newsId);
            if (alreadyFavorited)
                return BadRequest(new { message = "Article already in favorites" });

            var favorite = new UserFavoriteArticle
            {
                UserId = user.Id,
                ArticleId = newsId,
                AddedAt = DateTime.UtcNow
            };
             await _favoriteService.Add(favorite);

            return Ok(new { result = "Article added to favorites" });
        }

        // DELETE: api/user/favorites/favoriteId
        [HttpDelete("favorites/{favoriteId}")]
        public async Task<IActionResult> RemoveFromFavorites(int favoriteId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetCurrentUser(userId);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var favorite = await _favoriteService.GetFavoriteById(favoriteId);
            if (favorite == null)
                return NotFound(new { message = "Favorite not found" });

            if (favorite.UserId != user.Id)
                return Forbid();

            await _favoriteService.Remove(favoriteId);

            return Ok(new { result = "Article removed from favorites" });
        }

        // GET : api/user/favorites
        [HttpGet("favorites")]
		public async Task<IActionResult> GetUserFavorites()
		{
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetCurrentUser(userId);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var favorites = await _favoriteService.GetFavoritesByUser(user.Id); 
			return Ok(favorites);
		}
    }
}
