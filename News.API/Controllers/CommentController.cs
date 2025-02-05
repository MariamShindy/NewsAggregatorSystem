using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Contracts.NewsCatcher;
using News.Core.Dtos;
using News.Core.Entities;
using Comment = News.Core.Entities.Comment;

namespace News.API.Controllers
{
    [Authorize/*(Roles = "Admin,User")*/]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController(ICommentService _commentService,
        IUserService _userService ,INewsTwoService _newsService) : ControllerBase
    {
        // POST: api/comment/{newsId}/comments
        [HttpPost("{newsId}/comments")]
        public async Task<IActionResult> AddComment(string newsId, [FromBody] AddCommentDto model)
        {
            var article = await _newsService.GetNewsByIdAsync(newsId);
            if (article == null) return NotFound(new {result = "Article not found"});
            var user = await _userService.GetCurrentUserAsync();
            var comment = new Comment
            {
                Content = model.Content,
                UserId = user.Id,
                User = user,
                ArticleId = newsId,
                CreatedAt = DateTime.UtcNow,
               
            };
            await _commentService.AddAsync(comment);
            return Ok(new { result = "Comment added" });
        }
        // PUT: api/comment/comments/{id}
        [HttpPut("comments/{id}")]
        public async Task<IActionResult> EditComment(int id, [FromBody] UpdateCommentDto model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound("Comment not found.");
            if (comment.UserId != user.Id)
                return Forbid("You are not authorized to edit this comment.");

            comment.Content = model.Content;
            comment.CreatedAt = DateTime.UtcNow;
            await _commentService.UpdateAsync(comment);
            return Ok(new { message = "Comment updated successfully." });
        }

        // DELETE: api/comment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            await _commentService.DeleteAsync(id);
            return Ok(new { result = "Comment deleted" });
        }

        // GET: api/comment
        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _commentService.GetAllAsync();
            var formattedComments = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), 
                UserId = c.UserId,
                UserName = c.User.UserName,
                IsLocked = c.User.LockoutEnd.HasValue && c.User.LockoutEnd > DateTimeOffset.UtcNow,
                ProfilePicUrl = c.User.ProfilePicUrl
            });
            return Ok(formattedComments);
        }

        // GET: api/comment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var formattedComment = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), 
                UserId = comment.UserId,
                UserName = comment.User.UserName,
                IsLocked = comment.User.LockoutEnd.HasValue && comment.User.LockoutEnd > DateTimeOffset.UtcNow,
                ProfilePicUrl = comment.User.ProfilePicUrl

            };
            return Ok(formattedComment);
        }

        // GET: api/comment/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUserId(string userId)
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(userId);
            if (comments == null || !comments.Any())
                return NotFound("No comments found for this user.");

            var formattedComments = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), 
                UserId = c.UserId,
                UserName = c.User.UserName,
                IsLocked = c.User.LockoutEnd.HasValue && c.User.LockoutEnd > DateTimeOffset.UtcNow,
                ProfilePicUrl = c.User.ProfilePicUrl
            });

            return Ok(formattedComments);
        }

        // GET: api/comment/article/{articleId}
        [HttpGet("article/{articleId}")]
        public async Task<IActionResult> GetCommentsByArticleId(string articleId)
        {
            var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);
            if (comments == null || !comments.Any())
                return NotFound("No comments found for this article.");

            var formattedComments = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), 
                UserId = c.UserId,
                UserName = c.User.UserName,
                IsLocked = c.User.LockoutEnd.HasValue && c.User.LockoutEnd > DateTimeOffset.UtcNow,
                ProfilePicUrl = c.User.ProfilePicUrl
            });

            return Ok(formattedComments);
        }
    }
}
