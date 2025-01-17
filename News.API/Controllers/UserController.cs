﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;

namespace News.API.Controllers
{
    [Authorize(Roles ="User")]
	[Route("api/[controller]")]
	[ApiController]
	public class UserController(IMapper _mapper,
        IUserService _userService,IFavoriteService _favoriteService ,
        INewsService _newsService) : ControllerBase
	{
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var user = await _userService.GetCurrentUserAsync();
            var userInfo = _mapper.Map<UserDto>(user);
            return Ok(userInfo);
        }

        // PUT: api/user/me
        [HttpPut("me")]
        public async Task<IActionResult> EditUserInfo([FromBody] EditUserDto model)
        {
            var result = await _userService.UpdateUserAsync(model); 
            if (result.Succeeded)
                return Ok(new { result = "User updated successfully" }); //not updated profile pic

            return BadRequest(result.Errors);
        }

        // POST: api/user/send-feedback
        [HttpPost("send-feedback")]
        [AllowAnonymous]
        public async Task<IActionResult> SendFeedback([FromBody] FeedbackDto feedbackDto)
        {
            if (feedbackDto == null ||
                string.IsNullOrWhiteSpace(feedbackDto.FullName) ||
                string.IsNullOrWhiteSpace(feedbackDto.Email) ||
                string.IsNullOrWhiteSpace(feedbackDto.Subject) ||
                string.IsNullOrWhiteSpace(feedbackDto.Message))
            {
                return BadRequest(new { Status = "Error", Message = "All fields are required." });
            }
            try
            {
                bool isSent = await _userService.SendFeedbackAsync(feedbackDto); 
                return Ok(new { Status = "Success", Message = "Feedback received." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending feedback. {ex}");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while sending feedback. Please try again later." });
            }
        }
        // POST: api/user/send-survey
        [HttpPost("send-survey")]
        public async Task<IActionResult> SendSurvey([FromBody] SurveyDto surveyDto)
        {
            if (surveyDto == null )
                return BadRequest();
            try
            {
                bool isSent = await _userService.SendSurveyAsync(surveyDto);
                return Ok(new { Status = "Success", Message = "Survey received." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending survey. {ex}");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while sending survey. Please try again later." });
            }
        }


        // POST: api/user/favorites/{newsId}
        [HttpPost("favorites/{newsId}")]
        public async Task<IActionResult> AddToFavorites(string newsId)
        {
            var user = await _userService.GetCurrentUserAsync();
            var articleExists = await _newsService.CheckArticleExistsAsync(newsId);
            if (!articleExists)
                return NotFound(new { message = "Article not found" });
            
            var alreadyFavorited = await _favoriteService.IsArticleFavoritedAsync(user.Id, newsId);
            if (alreadyFavorited)
                return BadRequest(new { message = "Article already in favorites" });
            
            await _favoriteService.AddToFavoritesAsync(user.Id, newsId);
            return Ok(new { result = "Article added to favorites" });
        }

        // DELETE: api/user/favorites/{newsId}
        [HttpDelete("favorites/{newsId}")]
        public async Task<IActionResult> RemoveFromFavorites(string newsId)
        {
            var user = await _userService.GetCurrentUserAsync();
            await _favoriteService.RemoveFromFavoritesAsync(user.Id, newsId);
            return Ok(new { result = "Article removed from favorites" });
        }

        // GET : api/user/favorites
        [HttpGet("favorites")]
        public async Task<IActionResult> GetUserFavorites()
        {
            var user = await _userService.GetCurrentUserAsync();
            var favoriteArticles = await _favoriteService.GetFavoritesByUserAsync(user.Id);
            return Ok(favoriteArticles); 
        }


        // GET : api/user/set-preferred-categories
        [HttpPost("set-preferred-categories")]
        public async Task<IActionResult> SetPreferredCategories(SetPreferredCategoriesDto model)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user is null)
                return Unauthorized();
            try
            {
                await _userService.SetUserPreferredCategoriesAsync(user, model.CategoryNames);
                return Ok("Preferred categories updated successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  
            }
        }
        // GET : api/user/referred-categories
        [HttpGet("preferred-categories")]
        public async Task<IActionResult> GetUserPreferredCategories()
        {
            try
            {
                var categories = await _userService.GetUserPreferredCategoriesAsync();
                return Ok(categories);  
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);  
            }
        }
        #region Favorite before caching
        //// POST: api/user/favorites/{newsId}
        //[HttpPost("favorites/{newsId}")]
        //public async Task<IActionResult> AddToFavorites(string newsId)
        //{

        //    var user = await _userService.GetCurrentUser();
        //    var articleExists = await _newsService.CheckArticleExists(newsId);
        //    if (!articleExists)
        //        return NotFound(new { message = "Article not found" });

        //    var alreadyFavorited = await _favoriteService.IsArticleFavorited(user.Id, newsId);
        //    if (alreadyFavorited)
        //        return BadRequest(new { message = "Article already in favorites" });

        //    var favorite = new UserFavoriteArticle
        //    {
        //        UserId = user.Id,
        //        ArticleId = newsId,
        //        AddedAt = DateTime.UtcNow
        //    };

        //    await _favoriteService.Add(favorite);
        //    return Ok(new { result = "Article added to favorites" });
        //}

        //// DELETE: api/user/favorites/{favoriteId}
        //[HttpDelete("favorites/{favoriteId}")]
        //public async Task<IActionResult> RemoveFromFavorites(int favoriteId)
        //{
        //    var user = await _userService.GetCurrentUser();
        //    var favorite = await _favoriteService.GetFavoriteById(favoriteId);
        //    if (favorite == null)
        //        return NotFound(new { message = "Favorite not found" });
        //    if (favorite.UserId != user.Id)
        //        return Forbid();
        //    await _favoriteService.Remove(favoriteId);
        //    return Ok(new { result = "Article removed from favorites" });
        //}

        //// GET : api/user/favorites
        //[HttpGet("favorites")]
        //public async Task<IActionResult> GetUserFavorites()
        //{
        //    var user = await _userService.GetCurrentUser();
        //    var favorites = await _favoriteService.GetFavoritesByUser(user.Id);
        //    var favoriteDtos = _mapper.Map<List<FavoriteArticleDto>>(favorites);
        //    return Ok(favoriteDtos);
        //} 
        #endregion

    }
}
