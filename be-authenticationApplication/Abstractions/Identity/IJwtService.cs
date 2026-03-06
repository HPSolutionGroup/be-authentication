using be_authenticationDomain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace be_authenticationApplication.Abstractions.Identity
{
    public interface IJwtService
    {
        string GenerateJWTToken(User user, IList<string> roles, Guid branchId);
        public string GenerateRefreshToken();
        JsonWebKeySet GetJwks();
    }
}
