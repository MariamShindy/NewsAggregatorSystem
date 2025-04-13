namespace News.Infrastructure.Repositories
{
    public class GenericRepository<T> (ApplicationDbContext _dbContext) : IGenericRepository<T> where T : class
    {
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }
        public async Task<IReadOnlyList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (include != null)
                query = include(query); 

            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate) //should be sync
        {
            return  _dbContext.Set<T>().Where(predicate).AsEnumerable();
        }
        public async Task<T?> GetByIdAsync(int id)//should be sync
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        => await  _dbContext.Set<T>().AddAsync(entity);

        public async Task UpdateAsync(T entity)//should be sync
        => _dbContext.Set<T>().Update(entity);

        public async Task DeleteAsync(T entity)//should be sync
        => _dbContext.Set<T>().Remove(entity);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (include != null)
                query = include(query); 

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }
    }
}

