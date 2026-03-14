using HP.Authentication.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HP.Authentication.Application.Abstractions.Identity
{
    public interface IJwtService
    {
        string GenerateJWTToken(User user, IList<string> roles, Guid branchId);
        string GenerateRefreshToken();
        JsonWebKeySet GetJwks();
    }
}
