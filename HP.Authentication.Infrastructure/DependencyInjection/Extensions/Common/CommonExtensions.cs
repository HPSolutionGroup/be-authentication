using HP.Authentication.Application.Common;
using HP.Authentication.Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;

namespace HP.Authentication.Infrastructure.DependencyInjection.Extensions.Common
{
    public static class CommonExtensions
    {
        public static IServiceCollection AddCommonExtensions(this IServiceCollection services)
        {
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            return services;
        }
    }
}
