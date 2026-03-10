using be_authenticationApplication.Abstractions.Identity;
using be_authenticationApplication.Abstractions.Repository;
using be_authenticationApplication.Common;
using be_authenticationDomain.CustomException;
using be_authenticationDomain.Entities;
using Microsoft.Extensions.Configuration;

namespace be_authenticationInfrastructure.Integrations.Identity
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenManager(
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration,
            IRefreshTokenHasher refreshTokenHasher,
            IDateTimeProvider dateTimeProvider)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _refreshTokenHasher = refreshTokenHasher;
            _dateTimeProvider = dateTimeProvider;
        }


        // Lấy thời gian expiry refreshtoken trong appsettings
        private int RefreshExpiryDays => int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        #region Create Refresh Token

        public async Task CreateTokenAsync(
            Guid userId,
            string tokenString,
            string ipAddress,
            Guid? sessionId = null)
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException("UserId không hợp lệ.");

            if (string.IsNullOrWhiteSpace(tokenString))
                throw new CustomException.InvalidDataException("Refresh token không được để trống.");

            var now = _dateTimeProvider.UtcNow;

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = _refreshTokenHasher.Hash(tokenString),
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

        public async Task<User> VerifyAndRotateTokenAsync(
            string oldTokenString,
            string newTokenString,
            string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(oldTokenString))
                throw new CustomException.InvalidDataException("Refresh token cũ không được để trống.");

            if (string.IsNullOrWhiteSpace(newTokenString))
                throw new CustomException.InvalidDataException("Refresh token mới không được để trống.");

            var oldTokenHash = _refreshTokenHasher.Hash(oldTokenString);

            var tokenEntity = await _refreshTokenRepository
                .GetByTokenHashAsync(oldTokenHash, includeUser: true);

            if (tokenEntity == null)
                throw new CustomException.UnAuthorizedException(
                    "Refresh token không hợp lệ hoặc không tồn tại.");

            if (tokenEntity.IsRevoked)
            {
                await RevokeFamilyAsync(
                    tokenEntity.FamilyId,
                    ipAddress,
                    "Refresh token reuse detected");

                throw new CustomException.UnAuthorizedException(
                    "Phát hiện truy cập bất thường. Phiên đăng nhập đã bị thu hồi.");
            }

            if (tokenEntity.IsExpired)
                throw new CustomException.UnAuthorizedException(
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

            var now = _dateTimeProvider.UtcNow;

            tokenEntity.LastUsedAt = now;
            tokenEntity.RevokedAt = now;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.ReasonRevoked = "Rotated";

            var newRefreshToken = new RefreshToken
            {
                UserId = tokenEntity.UserId,
                Token = _refreshTokenHasher.Hash(newTokenString),
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

            return tokenEntity.User;
        }

        #endregion

        #region Revoke Single Token

        public async Task RevokeTokenAsync(
            string tokenString,
            string ipAddress,
            string reason = "Manual revoke")
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                throw new CustomException.InvalidDataException("Refresh token không được để trống.");

            var tokenHash = _refreshTokenHasher.Hash(tokenString);

            var tokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (tokenEntity == null)
                throw new CustomException.DataNotFoundException("Không tìm thấy refresh token.");

            if (!tokenEntity.IsActive)
                return;
            var now = _dateTimeProvider.UtcNow;

            tokenEntity.RevokedAt = now;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.ReasonRevoked = reason;

            _refreshTokenRepository.Update(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        #endregion

        #region Revoke Token Family

        public async Task RevokeFamilyAsync(
            Guid familyId,
            string ipAddress,
            string reason)
        {
            if (familyId == Guid.Empty)
                throw new CustomException.InvalidDataException("FamilyId không hợp lệ.");

            var tokens = await _refreshTokenRepository.GetActiveByFamilyIdAsync(familyId);

            if (!tokens.Any())
                return;
            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        #endregion

        #region Revoke Session Tokens

        public async Task RevokeSessionAsync(
            Guid sessionId,
            string ipAddress,
            string reason = "Session revoked")
        {
            if (sessionId == Guid.Empty)
                throw new CustomException.InvalidDataException("SessionId không hợp lệ.");

            var tokens = await _refreshTokenRepository.GetActiveBySessionIdAsync(sessionId);

            if (!tokens.Any())
                return;
            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        #endregion

        #region Revoke All User Tokens

        public async Task RevokeAllUserTokensAsync(
            Guid userId,
            string ipAddress,
            string reason = "Revoke all devices")
        {
            if (userId == Guid.Empty)
                throw new CustomException.InvalidDataException("UserId không hợp lệ.");

            var tokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId);

            if (!tokens.Any())
                return;
            var now = _dateTimeProvider.UtcNow;

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = reason;
            }

            _refreshTokenRepository.UpdateRange(tokens);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        #endregion
    }
}