using System.Linq.Expressions;

namespace HP.Authentication.Application.Abstractions.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(
            object id,
            CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(
            object[] keyValues,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        IQueryable<T> Query(bool asNoTracking = true);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void Update(T entity);

        void Delete(T entity);
    }
}
