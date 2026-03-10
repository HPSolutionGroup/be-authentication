namespace be_authenticationApplication.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
