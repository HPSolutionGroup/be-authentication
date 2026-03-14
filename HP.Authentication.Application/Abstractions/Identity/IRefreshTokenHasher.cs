namespace HP.Authentication.Application.Abstractions.Identity
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
    }
}
