using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Abstractions.Repository;
using HP.Authentication.Application.Common;
using HP.Authentication.Domain.CustomException;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using Microsoft.Extensions.Logging;

namespace HP.Authentication.Infrastructure.Integrations.Identity
{
    public class UserSessionManager : IUserSessionManager
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IRefreshTokenManager _refreshTokenManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UserSessionManager> _logger;
        private readonly IJsonLocalizationService _localizer;

        public UserSessionManager(
            IUserSessionRepository userSessionRepository,
            IRefreshTokenManager refreshTokenManager,
            IDateTimeProvider dateTimeProvider,
            ILogger<UserSessionManager> logger,
            IJsonLocalizationService localizer)
        {
            _userSessionRepository = userSessionRepository;
            _refreshTokenManager = refreshTokenManager;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _localizer = localizer;
        }

        #region Create Session
        public async Task<UserSession> CreateSessionAsync(
            Guid userId,
            string? ipAddress,
            string? userAgent,
            string? deviceName,
            string? deviceId = null)
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_USER));

            var now = _dateTimeProvider.UtcNow;

            // Kiểm tra DeviceId
            //
            if (!string.IsNullOrEmpty(deviceId))
            {
                var existingSession = await _userSessionRepository.GetActiveByDeviceAsync(userId, deviceId);

                if (existingSession != null)
                {
                    // Nếu thiết bị này đã có session, chúng ta "làm mới" nó
                    //
                    existingSession.IpAddress = ipAddress;
                    existingSession.UserAgent = userAgent;
                    existingSession.DeviceName = deviceName;
                    existingSession.LastSeenAt = now;

                    _userSessionRepository.Update(existingSession);
                    await _userSessionRepository.SaveChangesAsync();

                    _logger.LogInformation("Existing session updated for DeviceId={DeviceId}", deviceId);
                    return existingSession;
                }
            }

            var session = new UserSession
            {
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceName = deviceName,
                DeviceId = deviceId,
                CreatedAt = now,
                LastSeenAt = now
            };

            await _userSessionRepository.AddAsync(session);
            await _userSessionRepository.SaveChangesAsync();

            _logger.LogInformation(
                "User session created. SessionId={SessionId}, UserId={UserId}, DeviceName={DeviceName}, DeviceId={DeviceId}, IpAddress={IpAddress}",
                session.Id, userId, deviceName, deviceId, ipAddress);

            return session;
        }
        #endregion

        #region Touch Session
        public async Task TouchSessionAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_SESSION));

            var session = await _userSessionRepository.GetActiveByIdAsync(sessionId);

            if (session == null)
                return;

            session.LastSeenAt = _dateTimeProvider.UtcNow;

            _userSessionRepository.Update(session);
            await _userSessionRepository.SaveChangesAsync();

            _logger.LogDebug(
                "User session touched. SessionId={SessionId}, UserId={UserId}",
                session.Id, session.UserId);
        }
        #endregion

        #region Revoke Single Session
        public async Task RevokeSessionAsync(
            Guid sessionId,
            string? ipAddress,
            string reason = "Session revoked")
        {
            if (sessionId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_SESSION));

            var session = await _userSessionRepository.GetByIdAsync(sessionId);

            if (session == null)
                throw new CustomException.DataNotFoundException(
                    _localizer.Get("auth", AuthKeys.INVALID_SESSION));

            if (!session.IsActive)
            {
                _logger.LogDebug(
                    "Revoke session skipped: session already inactive. SessionId={SessionId}, UserId={UserId}",
                    session.Id, session.UserId);
                return;
            }

            var now = _dateTimeProvider.UtcNow;

            session.RevokedAt = now;
            session.RevokedReason = reason;

            _userSessionRepository.Update(session);
            await _userSessionRepository.SaveChangesAsync();

            await _refreshTokenManager.RevokeSessionAsync(sessionId, ipAddress ?? string.Empty, reason);

            _logger.LogInformation(
                "User session revoked. SessionId={SessionId}, UserId={UserId}, Reason={Reason}, IpAddress={IpAddress}",
                session.Id, session.UserId, reason, ipAddress);
        }
        #endregion

        #region Revoke All User Sessions
        public async Task RevokeAllUserSessionsAsync(
            Guid userId,
            string? ipAddress,
            string reason = "Revoke all sessions")
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_USER));

            var sessions = await _userSessionRepository.GetActiveByUserIdAsync(userId);

            if (!sessions.Any())
            {
                _logger.LogDebug(
                    "Revoke all sessions skipped: no active sessions. UserId={UserId}, IpAddress={IpAddress}",
                    userId, ipAddress);
                return;
            }

            var now = _dateTimeProvider.UtcNow;

            foreach (var session in sessions)
            {
                session.RevokedAt = now;
                session.RevokedReason = reason;
            }

            _userSessionRepository.UpdateRange(sessions);
            await _userSessionRepository.SaveChangesAsync();

            await _refreshTokenManager.RevokeAllUserTokensAsync(userId, ipAddress ?? string.Empty, reason);

            _logger.LogInformation(
                "All user sessions revoked. UserId={UserId}, Count={Count}, Reason={Reason}, IpAddress={IpAddress}",
                userId, sessions.Count, reason, ipAddress);
        }
        #endregion

        #region Get Active Sessions
        public async Task<List<UserSession>> GetActiveSessionsByUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_USER));

            return await _userSessionRepository.GetActiveByUserIdAsync(userId);
        }
        #endregion
    }
}
