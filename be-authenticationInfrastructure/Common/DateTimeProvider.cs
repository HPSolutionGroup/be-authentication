using be_authenticationApplication.Common;

namespace be_authenticationInfrastructure.Common
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
