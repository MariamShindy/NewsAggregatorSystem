using System.Linq.Expressions;

namespace News.Core.Contracts
{
    public interface IGenericRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
    }
}
