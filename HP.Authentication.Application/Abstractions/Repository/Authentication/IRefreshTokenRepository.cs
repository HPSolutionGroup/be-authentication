using HP.Authentication.Domain.Entities;

namespace HP.Authentication.Application.Abstractions.Repository.Authentication
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
