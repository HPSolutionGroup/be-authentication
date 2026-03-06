using be_authenticationApplication.Abstractions.Identity;
using be_authenticationDomain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace be_authenticationInfrastructure.Integrations.Identity
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly RSA _rsaPrivate;
        private readonly RSA _rsaPublic;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Nạp Private Key để ký (Sign)
            _rsaPrivate = RSA.Create();
            var privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
            if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
                throw new FileNotFoundException($"Missing Private Key at {privateKeyPath}");

            _rsaPrivate.ImportFromPem(File.ReadAllText(privateKeyPath).ToCharArray());

            // Nạp Public Key để xuất (JWKS)
            _rsaPublic = RSA.Create();
            var publicKeyPath = _configuration["Jwt:PublicKeyPath"];
            if (string.IsNullOrWhiteSpace(publicKeyPath) || !File.Exists(publicKeyPath))
                throw new FileNotFoundException($"Missing Public Key at {publicKeyPath}");

            _rsaPublic.ImportFromPem(File.ReadAllText(publicKeyPath).ToCharArray());
        }

        #region Generate Token
        // Token
        public string GenerateJWTToken(User user, IList<string> roles, Guid branchId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, string.Join(",", roles)),
                new Claim("name", user.Name ?? ""),
                new Claim("avatar", user.Avatar ?? ""),
                new Claim("branchId", branchId.ToString()),
                new Claim("userName", user.UserName ?? "")
            };

            var key = new RsaSecurityKey(_rsaPrivate)
            {
                KeyId = _configuration["Jwt:KeyId"]
            };

            // Chuyển sang thuật toán Asymmetric RS256
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Refresh Token
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        #endregion
        public JsonWebKeySet GetJwks()
        {
            var securityKey = new RsaSecurityKey(_rsaPublic)
            {
                KeyId = _configuration["Jwt:KeyId"]
            };

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);
            jwk.Use = "sig";
            jwk.Alg = SecurityAlgorithms.RsaSha256;

            return new JsonWebKeySet { Keys = { jwk } };
        }
    }
}
