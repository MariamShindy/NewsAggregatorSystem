using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController(ICommentService _commentService, IUserService _userService) : ControllerBase
    {
        // GET: api/comment
        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _commentService.GetAllAsync();
            return Ok(comments);
        }

        // GET: api/comment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // POST: api/comment/{newsId}/comments
        [HttpPost("{newsId}/comments")]
        public async Task<IActionResult> AddComment(string newsId, [FromBody] AddCommentDto model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var comment = new Comment
            {
                Content = model.Content,
                UserId = user.Id,
                User = user,
                ArticleId = newsId,
                CreatedAt = DateTime.UtcNow
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

            ////AUTOMAPPER
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
    }
}
