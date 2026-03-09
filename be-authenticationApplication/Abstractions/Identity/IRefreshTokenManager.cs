using be_authenticationDomain.Entities;

namespace be_authenticationApplication.Abstractions.Identity
{
    public interface IRefreshTokenManager
    {
        // Nhận chuỗi Token do Handler sinh ra và lưu mới vào DB
        Task CreateTokenAsync(Guid userId, string tokenString, string ipAddress, string userAgent, string deviceName);

        // Kiểm tra Token cũ, nếu hợp lệ thì thu hồi nó và lưu Token mới. Trả về thông tin User.
        Task<User> VerifyAndRotateTokenAsync(string oldTokenString, string newTokenString, string ipAddress, string userAgent);

        // Thu hồi (Revoke) Token bằng tay khi User đăng xuất
        Task RevokeTokenAsync(string tokenString, string ipAddress);
    }
}
