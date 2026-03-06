using be_authenticationApplication.Abstractions.Repository;
using be_authenticationInfrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace be_authenticationInfrastructure.Integrations.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _context;

        private IDbContextTransaction? _transaction;

        private Hashtable _repositories;

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<T>(_context);

                _repositories.Add(type, repository);
            }

            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync();

            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
