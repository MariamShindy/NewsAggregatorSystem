using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;

namespace News.Service.Services
{
    public class CommentService(IUnitOfWork _unitOfWork) : ICommentService
	{
		public async Task<IEnumerable<Comment>> GetAll()
		{
			return await _unitOfWork.Repository<Comment>().GetAllAsync();
		}

		public async Task<Comment> GetById(int id)
		{
			return await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
		}

		public async Task Add(Comment comment)
		{
			_unitOfWork.Repository<Comment>().Add(comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Update(Comment comment)
		{
			var existingComment =await _unitOfWork.Repository<Comment>().GetByIdAsync(comment.Id);
			if (existingComment == null) return;

			existingComment.Content = comment.Content; 
			_unitOfWork.Repository<Comment>().Update(existingComment);
            await _unitOfWork.CompleteAsync();
        }
        public async Task Delete(int id)
		{
			var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id);
			if (comment == null) return;
          
			_unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.CompleteAsync();

        }
    }
}
