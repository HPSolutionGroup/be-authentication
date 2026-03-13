namespace be_authenticationApplication.Common
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
