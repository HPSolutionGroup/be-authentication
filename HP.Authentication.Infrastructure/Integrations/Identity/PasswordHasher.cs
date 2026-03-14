using HP.Authentication.Application.Abstractions.Identity;
using HP.Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HP.Authentication.Infrastructure.Integrations.Identity
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
