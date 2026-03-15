using HP.Authentication.Domain.Entities;

namespace HP.Authentication.Application.Abstractions.Identity
{
    public interface IUserSessionManager
    {
        Task<UserSession> CreateSessionAsync(
            Guid userId,
            string? ipAddress,
            string? userAgent,
            string? deviceName,
            string? deviceId = null);

        Task TouchSessionAsync(Guid sessionId);

        Task RevokeSessionAsync(
            Guid sessionId,
            string? ipAddress,
            string reason = "Session revoked");

        Task RevokeAllUserSessionsAsync(
            Guid userId,
            string? ipAddress,
            string reason = "Revoke all sessions");

        Task<List<UserSession>> GetActiveSessionsByUserAsync(Guid userId);
    }
}
