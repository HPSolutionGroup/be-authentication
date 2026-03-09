using be_authenticationApplication.Abstractions.Identity;
using be_authenticationApplication.Abstractions.Repository;
using be_authenticationDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace be_authenticationInfrastructure.Integrations.Identity
{
    public class RefreshTokenManager : IRefreshTokenManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public RefreshTokenManager(
            IUnitOfWork unitOfWork,
            IConfiguration configuration
            ) 
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        #region 1. Tạo Refresh Token mới (Dùng lúc Login)
        public async Task CreateTokenAsync(Guid userId, string tokenString, string ipAddress, string userAgent, string deviceName)
        {
            var refreshExpiry = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = tokenString,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiry),
                CreatedByIp = ipAddress
            };

            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Refresh Token
        public async Task<User> VerifyAndRotateTokenAsync(string oldTokenString, string newTokenString, string ipAddress, string userAgent)
        {
            var tokenRepo = _unitOfWork.Repository<RefreshToken>();

            // 1. Tìm Token trong DB, bắt buộc Include User để tí nữa Handler dùng
            var tokenEntity = await tokenRepo.Query()
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == oldTokenString);

            if (tokenEntity == null)
                throw new Exception("Token không hợp lệ hoặc không tồn tại.");

            // 2. KỊCH BẢN BỊ HACK: Token gửi lên đã bị thu hồi trước đó
            if (tokenEntity.IsRevoked)
            {
                // Gọi hàm đệ quy để khóa toàn bộ các Token con cháu
                await RevokeDescendantTokens(tokenEntity, ipAddress);
                tokenRepo.Update(tokenEntity);
                await _unitOfWork.SaveChangesAsync();

                // Quăng Exception để Handler bắt và trả về HTTP 401 Unauthorized
                throw new Exception("Phát hiện truy cập bất thường. Phiên đăng nhập đã bị hủy để bảo vệ tài khoản!");
            }

            // 3. KỊCH BẢN HẾT HẠN
            if (tokenEntity.IsExpired)
                throw new Exception("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

            // ==========================================
            // 4. TOKEN HỢP LỆ -> TIẾN HÀNH XOAY VÒNG
            // ==========================================

            // Thu hồi token cũ và trỏ tới token mới
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedByIp = ipAddress;
            

            // Tạo token mới
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = tokenEntity.UserId,
                Token = newTokenString,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            };

            tokenEntity.ReplacedByTokenId = newRefreshTokenEntity.Id;

            // Lưu cả 2 sự thay đổi vào DB trong cùng 1 Transaction
            await tokenRepo.AddAsync(newRefreshTokenEntity);
            tokenRepo.Update(tokenEntity);

            await _unitOfWork.SaveChangesAsync();

            // Trả đối tượng User về cho Handler để nó tiếp tục gọi JwtService sinh JWT
            return tokenEntity.User;
        }
        #endregion

        #region Revoke Token 
        public async Task RevokeTokenAsync(string tokenString, string ipAddress)
        {
            var tokenRepo = _unitOfWork.Repository<RefreshToken>();

            var tokenEntity = await tokenRepo.Query()
                .SingleOrDefaultAsync(x => x.Token == tokenString);

            // Nếu không tìm thấy hoặc Token đã bị thu hồi/hết hạn rồi thì không làm gì cả
            if (tokenEntity == null || !tokenEntity.IsActive)
                return;

            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedByIp = ipAddress;

            tokenRepo.Update(tokenEntity);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Helper
        private async Task RevokeDescendantTokens(RefreshToken token, string ipAddress)
        {
            if (token.ReplacedByTokenId != null)
            {
                var tokenRepo = _unitOfWork.Repository<RefreshToken>();

                var childToken = await tokenRepo.Query()
                    .SingleOrDefaultAsync(x => x.Id == token.ReplacedByTokenId);

                if (childToken != null)
                {
                    if (childToken.IsActive)
                    {
                        childToken.RevokedAt = DateTime.UtcNow;
                        childToken.RevokedByIp = ipAddress;
                        tokenRepo.Update(childToken);
                    }
                    else
                    {
                        await RevokeDescendantTokens(childToken, ipAddress);
                    }
                }
            }
        }
        #endregion
    }
}