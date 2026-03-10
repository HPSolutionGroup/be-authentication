namespace be_authenticationApplication.Abstractions.Identity
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
    }
}
