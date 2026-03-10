using be_authenticationApplication.Common;
using Microsoft.AspNetCore.Http;

namespace be_authenticationInfrastructure.Common
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return "unknown";

            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xForwardedFor))
            {
                var ip = xForwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                    return ip;
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp))
                return xRealIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return "unknown";

            return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        }
    }
}
