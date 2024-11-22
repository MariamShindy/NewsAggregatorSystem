using News.Core.Entities;

namespace News.Core.Contracts
{
	public interface ICommentService
	{
		Task<IEnumerable<Comment>> GetAll();       
		Task<Comment> GetById(int id);             
		Task Add(Comment comment);
		Task Update(Comment comment);        
		Task Delete(int id);
	}
}
