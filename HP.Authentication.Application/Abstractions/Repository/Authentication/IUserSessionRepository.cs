using HP.Authentication.Domain.Entities;

namespace HP.Authentication.Application.Abstractions.Repository.Authentication
{
    public interface IUserSessionRepository
    {
        Task AddAsync(UserSession session);
        void Update(UserSession session);
        void UpdateRange(IEnumerable<UserSession> sessions);
        Task SaveChangesAsync();

        Task<UserSession?> GetByIdAsync(Guid sessionId);
        Task<UserSession?> GetActiveByIdAsync(Guid sessionId);
        Task<List<UserSession>> GetActiveByUserIdAsync(Guid userId);
        Task<UserSession?> GetActiveByDeviceAsync(Guid userId, string deviceId);
    }
}
