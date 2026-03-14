namespace HP.Authentication.Application.Abstractions.Identity
{
    public record PasswordVerificationOutcome(bool IsSuccess, bool RequiresRehash);
}
