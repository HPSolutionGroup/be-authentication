using HP.Authentication.Application.Abstractions.Identity;
using System.Security.Cryptography;
using System.Text;

namespace HP.Authentication.Infrastructure.Integrations.Identity
{
    public class RefreshTokenHasher : IRefreshTokenHasher
    {
        public string Hash(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}

