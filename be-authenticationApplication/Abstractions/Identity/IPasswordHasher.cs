using be_authenticationDomain.Entities;

namespace be_authenticationApplication.Abstractions.Identity
{
    public interface IPasswordHasher
    {
        PasswordVerificationOutcome VerifyPassword(string providedPassword, string hashedPassword, User user);
        string HashPassword(User user, string password);
    }
}
