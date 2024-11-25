using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;

namespace News.Service.Services
{
    public class CommentService(ILogger<CommentService> _logger,IUnitOfWork _unitOfWork) : ICommentService
	{
		public async Task<IEnumerable<Comment>> GetAll()
		{
            _logger.LogInformation("CommentService --> GetAll called");
            return await _unitOfWork.Repository<Comment>().GetAllAsync();
		}

		public async Task<Comment> GetById(int id)
		{
            _logger.LogInformation($"CommentService --> GetById with id : {id} called");
            return await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
		}

		public async Task Add(Comment comment)
		{
            _logger.LogInformation("CommentService --> Add called");
            _unitOfWork.Repository<Comment>().Add(comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Update(Comment comment)
		{
            _logger.LogInformation("CommentService --> Update called");
            var existingComment =await _unitOfWork.Repository<Comment>().GetByIdAsync(comment.Id);
            if (existingComment == null)
            {
                _logger.LogWarning("CommentService --> Update --> existingComment is not found");
                return; 
            }
			existingComment.Content = comment.Content; 
			_unitOfWork.Repository<Comment>().Update(existingComment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("CommentService --> Update succeeded");
        }
        public async Task Delete(int id)
		{
            _logger.LogInformation($"CommentService --> Delete with id : {id} called");

            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
			if (comment == null) return;
          
			_unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"CommentService --> Delete with id : {id} succeeded");
        }
    }
}
