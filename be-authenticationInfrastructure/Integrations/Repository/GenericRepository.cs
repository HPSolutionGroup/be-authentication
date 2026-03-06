using be_authenticationApplication.Abstractions.Repository;
using be_authenticationInfrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace be_authenticationInfrastructure.Integrations.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly MyDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(MyDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            // FindAsync được tối ưu hóa rất tốt trong EF Core để tìm kiếm theo Primary Key
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            // Dùng ToListAsync để thực thi câu lệnh SQL và trả về data
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            // Lọc dữ liệu theo điều kiện truyền vào
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public IQueryable<TEntity> Query()
        {
            // Trả về IQueryable để tầng trên có thể linh hoạt build query (Include, OrderBy...)
            return _dbSet.AsQueryable();
        }

        public async Task AddAsync(TEntity entity)
        {
            // Chỉ thêm vào tracking trên RAM, UnitOfWork sẽ gọi SaveChanges sau
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            // Đánh dấu entity là Modified trên RAM
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            // Đánh dấu entity là Deleted trên RAM
            _dbSet.Remove(entity);
        }
    }
}
