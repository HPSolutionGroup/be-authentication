namespace be_authenticationApplication.Common
{
    public interface IUserContext
    {
        string GetIpAddress();
        string GetUserAgent();
    }
}
