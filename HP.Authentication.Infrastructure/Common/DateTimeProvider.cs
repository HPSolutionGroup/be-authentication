using HP.Authentication.Application.Common;

namespace HP.Authentication.Infrastructure.Common
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
