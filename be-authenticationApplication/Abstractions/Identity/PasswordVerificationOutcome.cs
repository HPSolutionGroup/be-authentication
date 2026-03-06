namespace be_authenticationApplication.Abstractions.Identity
{
    public record PasswordVerificationOutcome(bool IsSuccess, bool RequiresRehash);
}
