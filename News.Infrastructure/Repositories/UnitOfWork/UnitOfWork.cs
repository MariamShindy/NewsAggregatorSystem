using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Infrastructure.Data;
using System.Collections;

namespace News.Infrastructure.Repositories.UnitOfWork
{
    public class UnitOfWork(ApplicationDbContext _dbContext, Hashtable _repositories) : IUnitOfWork
    {
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var key = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(key))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(key, repository);
            }
            return _repositories[key] as IGenericRepository<TEntity>;
        }
        public async Task<int> CompleteAsync()
           => await _dbContext.SaveChangesAsync();

        public async ValueTask DisposeAsync()
            => await _dbContext.DisposeAsync();
    }
}
