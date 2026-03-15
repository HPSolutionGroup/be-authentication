namespace HP.Authentication.Application.Common
{
    public interface IUserContext
    {
        string GetIpAddress();
        string GetUserAgent();
        string GetDeviceName();
        string GetDeviceId();
    }
}
