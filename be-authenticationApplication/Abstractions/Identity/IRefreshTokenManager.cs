using be_authenticationDomain.Entities;

namespace be_authenticationApplication.Abstractions.Identity
{
    public interface IRefreshTokenManager
    {
        Task CreateTokenAsync(
            Guid userId,
            string tokenString,
            string ipAddress,
            Guid? sessionId = null);

        Task<User> VerifyAndRotateTokenAsync(
            string oldTokenString,
            string newTokenString,
            string ipAddress);

        Task RevokeTokenAsync(
            string tokenString,
            string ipAddress,
            string reason = "Manual revoke");

        Task RevokeFamilyAsync(
            Guid familyId,
            string ipAddress,
            string reason);

        Task RevokeSessionAsync(
            Guid sessionId,
            string ipAddress,
            string reason = "Session revoked");

        Task RevokeAllUserTokensAsync(
            Guid userId,
            string ipAddress,
            string reason = "Revoke all devices");
    }
}
