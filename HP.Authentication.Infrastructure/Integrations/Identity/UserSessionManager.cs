using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Abstractions.Repository.Authentication;
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
        /// <summary>
        /// Create a new login session for a user, or refresh the existing active session
        /// if the same device is already registered.
        /// - Validate the user id.
        /// - If DeviceId is provided, try to find an active session on that device.
        /// - If found, update session metadata (IP, UserAgent, DeviceName, LastSeenAt).
        /// - Otherwise, create a brand new session.
        ///
        /// Tạo mới một phiên đăng nhập cho user, hoặc làm mới session đang active
        /// nếu cùng một thiết bị đã tồn tại trước đó.
        /// - Kiểm tra user id hợp lệ.
        /// - Nếu có DeviceId, tìm session active theo thiết bị đó.
        /// - Nếu đã tồn tại, cập nhật thông tin session (IP, UserAgent, DeviceName, LastSeenAt).
        /// - Nếu chưa có, tạo session mới hoàn toàn.
        /// </summary>
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
        /// <summary>
        /// Update the last seen time of an active session.
        /// Usually called when the user performs a valid request to keep the session alive.
        /// - Validate the session id.
        /// - Load the active session.
        /// - If session exists, update LastSeenAt to current UTC time.
        ///
        /// Cập nhật thời gian hoạt động gần nhất của một session đang active.
        /// Thường được gọi khi user thực hiện request hợp lệ để giữ session còn sống.
        /// - Kiểm tra session id hợp lệ.
        /// - Lấy session đang active.
        /// - Nếu tồn tại, cập nhật LastSeenAt về thời điểm UTC hiện tại.
        /// </summary>
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
        /// <summary>
        /// Revoke a single user session and invalidate all active refresh tokens
        /// associated with that session.
        /// - Validate the session id.
        /// - Ensure the session exists.
        /// - If the session is already inactive, skip.
        /// - Mark the session as revoked with reason.
        /// - Revoke all refresh tokens bound to this session.
        ///
        /// Thu hồi một session cụ thể của user và vô hiệu hóa toàn bộ refresh token
        /// còn active thuộc session đó.
        /// - Kiểm tra session id hợp lệ.
        /// - Đảm bảo session tồn tại.
        /// - Nếu session đã inactive thì bỏ qua.
        /// - Đánh dấu session là revoked kèm lý do.
        /// - Thu hồi tất cả refresh token gắn với session này.
        /// </summary>
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

            // Revoke all refresh tokens that belong to this session
            // to prevent this device from refreshing access tokens anymore.
            // Thu hồi toàn bộ refresh token thuộc session này
            // để thiết bị không thể tiếp tục refresh access token.
            await _refreshTokenManager.RevokeSessionAsync(sessionId, ipAddress ?? string.Empty, reason);

            _logger.LogInformation(
                "User session revoked. SessionId={SessionId}, UserId={UserId}, Reason={Reason}, IpAddress={IpAddress}",
                session.Id, session.UserId, reason, ipAddress);
        }
        #endregion

        #region Revoke All User Sessions
        /// <summary>
        /// Revoke all active sessions of a user and invalidate all active refresh tokens
        /// across all devices.
        /// - Validate the user id.
        /// - Load all active sessions of the user.
        /// - If no active sessions exist, skip.
        /// - Mark all sessions as revoked with the same reason.
        /// - Revoke all active refresh tokens of the user.
        ///
        /// Thu hồi toàn bộ session đang active của một user và vô hiệu hóa toàn bộ
        /// refresh token còn active trên tất cả thiết bị.
        /// - Kiểm tra user id hợp lệ.
        /// - Lấy toàn bộ session active của user.
        /// - Nếu không có session active thì bỏ qua.
        /// - Đánh dấu tất cả session là revoked cùng một lý do.
        /// - Thu hồi toàn bộ refresh token active của user.
        /// </summary>
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
        /// <summary>
        /// Get all active sessions of a user.
        /// Used for session management screens such as “logged-in devices”.
        /// - Validate the user id.
        /// - Return all sessions that are not revoked.
        ///
        /// Lấy toàn bộ session đang active của một user.
        /// Dùng cho màn hình quản lý thiết bị đăng nhập hoặc danh sách phiên hoạt động.
        /// - Kiểm tra user id hợp lệ.
        /// - Trả về tất cả session chưa bị revoke.
        /// </summary>
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
