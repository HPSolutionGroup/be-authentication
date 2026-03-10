using be_authenticationDomain.Entities;

namespace be_authenticationApplication.Abstractions.Repository
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        void Update(RefreshToken token);
        void UpdateRange(IEnumerable<RefreshToken> tokens);
        Task SaveChangesAsync();

        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false);
        Task<RefreshToken?> GetByIdAsync(Guid id);
        Task<List<RefreshToken>> GetActiveByFamilyIdAsync(Guid familyId);
        Task<List<RefreshToken>> GetActiveBySessionIdAsync(Guid sessionId);
        Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
    }
}
