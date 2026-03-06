using be_authenticationApplication.Abstractions.Identity;
using be_authenticationDomain.Entities;
using Microsoft.AspNetCore.Identity;

namespace be_authenticationInfrastructure.Integrations.Identity
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<User> _passwordHasher;

        public PasswordHasher()
        {
            _passwordHasher = new PasswordHasher<User>();
        }

        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public PasswordVerificationOutcome VerifyPassword(string providedPassword, string hashedPassword, User user)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

            return new PasswordVerificationOutcome(
                IsSuccess: result != PasswordVerificationResult.Failed,
                RequiresRehash: result == PasswordVerificationResult.SuccessRehashNeeded
            );
        }
    }
}
