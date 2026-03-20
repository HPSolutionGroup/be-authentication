using HP.Authentication.Application.Abstractions.Repository.GenericRepository;
using HP.Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HP.Authentication.Infrastructure.Integrations.Repository.GenericRepository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        protected readonly MyDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(MyDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(id);
            return await _dbSet.FindAsync(new[] { id }, cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(
            object[] keyValues,
            CancellationToken cancellationToken = default)
        {
            if (keyValues is null || keyValues.Length == 0)
                throw new ArgumentException("Key values cannot be null or empty.", nameof(keyValues));

            return await _dbSet.FindAsync(keyValues, cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public IQueryable<TEntity> Query(bool asNoTracking = true)
        {
            return asNoTracking
                ? _dbSet.AsNoTracking()
                : _dbSet;
        }

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return _dbSet.AddAsync(entity, cancellationToken).AsTask();
        }

        public void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
        }
    }
}
