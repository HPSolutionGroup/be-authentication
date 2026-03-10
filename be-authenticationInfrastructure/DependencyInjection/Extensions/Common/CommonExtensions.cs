using be_authenticationApplication.Common;
using be_authenticationInfrastructure.Common;
using Microsoft.Extensions.DependencyInjection;

namespace be_authenticationInfrastructure.DependencyInjection.Extensions.Common
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
