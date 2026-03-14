namespace HP.Authentication.Application.Abstractions.Repository
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
