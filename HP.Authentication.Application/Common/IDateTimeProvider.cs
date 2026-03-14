namespace HP.Authentication.Application.Common
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
