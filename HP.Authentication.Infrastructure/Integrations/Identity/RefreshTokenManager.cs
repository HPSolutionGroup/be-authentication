using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Application.Abstractions.Repository;
using HP.Authentication.Application.Common;
using HP.Authentication.Domain.CustomException;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HP.Authentication.Infrastructure.Integrations.Identity
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<RefreshTokenManager> _logger;
        private readonly IJsonLocalizationService _localizer;

        public RefreshTokenManager(
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration,
            IRefreshTokenHasher refreshTokenHasher,
            IDateTimeProvider dateTimeProvider,
            ILogger<RefreshTokenManager> logger,
            IJsonLocalizationService localizer)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _refreshTokenHasher = refreshTokenHasher;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _localizer = localizer;
        }

        // Lấy thời gian expiry refreshtoken trong appsettings
        private int RefreshExpiryDays => int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        #region Create Refresh Token

        /// <summary> 
        /// Create a new refresh token for a user and persist it to the database.
        /// - Hash the refresh token before saving.
        /// - Set created time, expiration time, and creator IP.
        /// - Generate a new FamilyId to start a refresh token chain.
        /// - Optionally attach the token to a login session.
        /// 
        /// Tạo mới một refresh token cho user và lưu vào database.
        /// - Hash refresh token trước khi lưu.
        /// - Gán thời gian tạo, thời gian hết hạn, IP tạo token.
        /// - Tạo mới FamilyId để bắt đầu một chuỗi refresh token.
        /// - Có thể gắn với SessionId nếu hệ thống quản lý phiên đăng nhập theo thiết bị.
        /// </summary>

        public async Task CreateTokenAsync(
            Guid userId,
            string tokenString,
            string ipAddress,
            Guid? sessionId = null)
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_USER));

            if (string.IsNullOrWhiteSpace(tokenString))
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED));

            var now = _dateTimeProvider.UtcNow;

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = _refreshTokenHasher.Hash(tokenString),
                CreatedAt = now,
                ExpiresAt = now.AddDays(RefreshExpiryDays),
                CreatedByIp = ipAddress,
                FamilyId = Guid.NewGuid(),
                SessionId = sessionId
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        #endregion

        #region Verify And Rotate Refresh Token
        ///<summary>
        /// Verify the current refresh token and rotate to a new one.
        /// - Validate the old token.
        /// - Detect revoked/reused token.
        /// - Revoke the old token with reason "Rotated".
        /// - Create a new token in the same family.
        /// - Return the user for generating a new access token.
        ///
        /// Xác thực refresh token hiện tại và xoay vòng sang token mới.
        /// - Kiểm tra token cũ có hợp lệ không.
        /// - Phát hiện token đã bị revoke hoặc bị reuse.
        /// - Thu hồi token cũ với lý do "Rotated".
        /// - Tạo token mới trong cùng family.
        /// - Trả về user để sinh access token mới.
        /// </summary>

        public async Task<User> VerifyAndRotateTokenAsync(
            string oldTokenString,
            string newTokenString,
            string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(oldTokenString))
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED));

            if (string.IsNullOrWhiteSpace(newTokenString))
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED));

            var oldTokenHash = _refreshTokenHasher.Hash(oldTokenString);

            var tokenEntity = await _refreshTokenRepository
                .GetByTokenHashAsync(oldTokenHash, includeUser: true);

            if (tokenEntity == null)
                throw new CustomException.UnAuthorizedException(
                    _localizer.Get("auth", AuthKeys.INVALID_REFRESH_TOKEN));

            if (tokenEntity.IsRevoked)
                throw new CustomException.UnAuthorizedException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REUSE_DETECTED));

            if (tokenEntity.IsExpired)
                throw new CustomException.UnAuthorizedException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_EXPIRED));

            var now = _dateTimeProvider.UtcNow;

            tokenEntity.LastUsedAt = now;
            tokenEntity.RevokedAt = now;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.ReasonRevoked = "Rotated";

            var newRefreshToken = new RefreshToken
            {
                UserId = tokenEntity.UserId,
                TokenHash = _refreshTokenHasher.Hash(newTokenString),
                CreatedAt = now,
                ExpiresAt = now.AddDays(RefreshExpiryDays),
                CreatedByIp = ipAddress,
                FamilyId = tokenEntity.FamilyId,
                ParentTokenId = tokenEntity.Id,
                SessionId = tokenEntity.SessionId
            };

            tokenEntity.ReplacedByTokenId = newRefreshToken.Id;

            _refreshTokenRepository.Update(tokenEntity);
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogDebug(
                "Refresh token rotated successfully. UserId={UserId}, OldTokenId={OldTokenId}, NewTokenId={NewTokenId}, FamilyId={FamilyId}, SessionId={SessionId}, IpAddress={IpAddress}",
                tokenEntity.UserId,
                tokenEntity.Id,
                newRefreshToken.Id,
                tokenEntity.FamilyId,
                tokenEntity.SessionId,
                ipAddress);

            return tokenEntity.User;
        }

        #endregion

        #region Revoke Single Token
        /// <summary>
        /// Revoke a single refresh token.
        /// Used for current-device logout or manual token invalidation.
        ///
        /// Thu hồi một refresh token cụ thể.
        /// Dùng cho logout thiết bị hiện tại hoặc thu hồi token thủ công.
        /// </summary>

        public async Task RevokeTokenAsync(
            string tokenString,
            string ipAddress,
            string reason = "Manual revoke")
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_REQUIRED));

            var tokenHash = _refreshTokenHasher.Hash(tokenString);

            var tokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (tokenEntity == null)
                throw new CustomException.DataNotFoundException(
                    _localizer.Get("auth", AuthKeys.REFRESH_TOKEN_NOT_FOUND));

            if (!tokenEntity.IsActive)
            {
                _logger.LogDebug(
                    "Revoke token skipped: token is already inactive. TokenId={TokenId}, UserId={UserId}, IpAddress={IpAddress}",
                    tokenEntity.Id, tokenEntity.UserId, ipAddress);

                return;
            }

            var now = _dateTimeProvider.UtcNow;

            tokenEntity.RevokedAt = now;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.ReasonRevoked = reason;

            _refreshTokenRepository.Update(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogDebug(
                "Refresh token revoked successfully. TokenId={TokenId}, UserId={UserId}, Reason={Reason}, IpAddress={IpAddress}",
                tokenEntity.Id, tokenEntity.UserId, reason, ipAddress);
        }

        #endregion

        #region Revoke Token Family
        /// <summary>
        /// Revoke all active refresh tokens in the same family.
        /// Typically used when token reuse is detected.
        ///
        /// Thu hồi toàn bộ refresh token còn active trong cùng một family.
        /// Thường dùng khi phát hiện token bị reuse.
        /// </summary>

        public async Task RevokeFamilyAsync(
            Guid familyId,
            string ipAddress,
            string reason)
        {
            if (familyId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_REQUEST));

            var tokens = await _refreshTokenRepository.GetActiveByFamilyIdAsync(familyId);

            if (!tokens.Any())
            {
                _logger.LogDebug(
                    "Revoke token family skipped: no active tokens found. FamilyId={FamilyId}, IpAddress={IpAddress}",
                    familyId, ipAddress);

                return;
            }

            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Refresh token family revoked. FamilyId={FamilyId}, Count={Count}, Reason={Reason}, IpAddress={IpAddress}",
                familyId, tokens.Count, reason, ipAddress);
        }

        #endregion

        #region Revoke Session Tokens
        /// <summary>
        /// Revoke all active refresh tokens belonging to a specific session.
        /// Typically used for single-device logout.
        ///
        /// Thu hồi toàn bộ refresh token còn active thuộc một session cụ thể.
        /// Thường dùng khi logout một thiết bị cụ thể.
        /// </summary>

        public async Task RevokeSessionAsync(
            Guid sessionId,
            string ipAddress,
            string reason = "Session revoked")
        {
            if (sessionId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_SESSION));

            var tokens = await _refreshTokenRepository.GetActiveBySessionIdAsync(sessionId);

            if (!tokens.Any())
            {
                _logger.LogDebug(
                    "Revoke session tokens skipped: no active tokens found. SessionId={SessionId}, IpAddress={IpAddress}",
                    sessionId, ipAddress);

                return;
            }

            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Session tokens revoked successfully. SessionId={SessionId}, Count={Count}, Reason={Reason}, IpAddress={IpAddress}",
                sessionId, tokens.Count, reason, ipAddress);
        }

        #endregion

        #region Revoke All User Tokens
        /// <summary>
        /// Revoke all active refresh tokens of a user.
        /// Typically used for logout-all-devices or account compromise handling.
        ///
        /// Thu hồi toàn bộ refresh token còn active của một user.
        /// Thường dùng khi logout tất cả thiết bị hoặc xử lý tài khoản bị nghi ngờ lộ.
        /// </summary>

        public async Task RevokeAllUserTokensAsync(
            Guid userId,
            string ipAddress,
            string reason = "Revoke all devices")
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException(
                    _localizer.Get("auth", AuthKeys.INVALID_USER));

            var tokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId);

            if (!tokens.Any())
            {
                _logger.LogDebug(
                    "Revoke all user tokens skipped: no active tokens found. UserId={UserId}, IpAddress={IpAddress}",
                    userId, ipAddress);

                return;
            }

            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogInformation(
                "All active refresh tokens for user were revoked. UserId={UserId}, Count={Count}, Reason={Reason}, IpAddress={IpAddress}",
                userId, tokens.Count, reason, ipAddress);
        }

        #endregion
    }
}
