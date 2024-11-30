using Microsoft.EntityFrameworkCore;
using News.Core.Contracts;
using News.Infrastructure.Data;
using System.Linq.Expressions;

namespace News.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
        {
            return  _dbContext.Set<T>().Where(predicate).AsEnumerable();
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        =>  _dbContext.Set<T>().Add(entity);

        public async Task UpdateAsync(T entity)
        => _dbContext.Set<T>().Update(entity);

        public async Task DeleteAsync(T entity)
        => _dbContext.Set<T>().Remove(entity);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }
    }
}

