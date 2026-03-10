using be_authenticationApplication.Abstractions.Repository;
using be_authenticationDomain.Entities;
using be_authenticationInfrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_authenticationInfrastructure.Integrations.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MyDbContext _context;

        public RefreshTokenRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.Set<RefreshToken>().AddAsync(token);
        }

        public void Update(RefreshToken token)
        {
            _context.Set<RefreshToken>().Update(token);
        }

        public void UpdateRange(IEnumerable<RefreshToken> tokens)
        {
            _context.Set<RefreshToken>().UpdateRange(tokens);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false)
        {
            IQueryable<RefreshToken> query = _context.Set<RefreshToken>();

            if (includeUser)
            {
                query = query.Include(x => x.User);
            }

            return await query.FirstOrDefaultAsync(x => x.Token == tokenHash);
        }

        public async Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<RefreshToken>> GetActiveByFamilyIdAsync(Guid familyId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.FamilyId == familyId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<RefreshToken>> GetActiveBySessionIdAsync(Guid sessionId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.SessionId == sessionId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.UserId == userId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
