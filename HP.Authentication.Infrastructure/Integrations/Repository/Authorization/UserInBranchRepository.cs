using HP.Authentication.Application.Abstractions.Repository.Authorization;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HP.Authentication.Infrastructure.Integrations.Repository.Authorization
{
    public class UserInBranchRepository : IUserInBranchRepository
    {
        private readonly MyDbContext _context;

        public UserInBranchRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserInBranch>> GetBranchesOfUserAsync(Guid userId)
        {
            return await _context.Set<UserInBranch>()
                .Where(x => x.UserId == userId
                         && x.Branch.IsActive
                         && !x.Branch.IsDeleted)
                .Include(x => x.Branch)
                .ToListAsync();
        }
    }
}
