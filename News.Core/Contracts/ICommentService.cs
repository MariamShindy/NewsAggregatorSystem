using News.Core.Entities;

namespace News.Core.Contracts
{
	public interface ICommentService
	{
		Task<IEnumerable<Comment>> GetAllAsync();       
		Task<Comment> GetByIdAsync(int id);             
		Task AddAsync(Comment comment);
		Task UpdateAsync(Comment comment);        
		Task DeleteAsync(int id);
        Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(string userId);
    }
}
