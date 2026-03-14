using HP.Authentication.Domain.Entities;

namespace HP.Authentication.Application.Abstractions.Identity
{
    public interface IPasswordHasher
    {
        PasswordVerificationOutcome VerifyPassword(string providedPassword, string hashedPassword, User user);
        string HashPassword(User user, string password);
    }
}
