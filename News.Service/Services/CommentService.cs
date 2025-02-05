using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.NewsCatcher;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;

namespace News.Service.Services
{
    public class CommentService(ILogger<CommentService> _logger,
        IUnitOfWork _unitOfWork) : ICommentService
    {
        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            _logger.LogInformation("CommentService --> GetAll called");
            return await _unitOfWork.Repository<Comment>()
        .GetAllAsync(query => query.Include(c => c.User));
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            _logger.LogInformation($"CommentService --> GetById with id : {id} called");
            var comment = await _unitOfWork.Repository<Comment>()
                .FindAsync(c => c.Id == id, query => query.Include(c => c.User));

            return comment.FirstOrDefault() ?? new Comment();
        }
        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(string userId)
        {
            _logger.LogInformation($"CommentService --> GetCommentsByUserIdAsync with userId : {userId} called");
            return await _unitOfWork.Repository<Comment>()
           .FindAsync(c => c.UserId == userId, query => query.Include(c => c.User));
        }
        public async Task AddAsync(Comment comment)
        {
            _logger.LogInformation("CommentService --> Add called");
            await _unitOfWork.Repository<Comment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            _logger.LogInformation("CommentService --> Update called");
            var existingComment = await _unitOfWork.Repository<Comment>().GetByIdAsync(comment.Id);
            if (existingComment == null)
            {
                _logger.LogWarning("CommentService --> Update --> existingComment is not found");
                return;
            }
            existingComment.Content = comment.Content;
            await _unitOfWork.Repository<Comment>().UpdateAsync(existingComment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("CommentService --> Update succeeded");
        }
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"CommentService --> Delete with id : {id} called");

            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
            if (comment == null) return;

            await _unitOfWork.Repository<Comment>().DeleteAsync(comment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"CommentService --> Delete with id : {id} succeeded");
        }
        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(string articleId)
        {
            _logger.LogInformation($"CommentService --> GetCommentsByArticleIdAsync with articleId: {articleId} called");

            return await _unitOfWork.Repository<Comment>()
                .FindAsync(c => c.ArticleId == articleId, query => query.Include(c => c.User));
        }
    }
}
