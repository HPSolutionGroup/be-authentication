using HP.Authentication.Infrastructure.DependencyInjection.Extensions.Authentication;
using HP.Authentication.Infrastructure.DependencyInjection.Extensions.Common;
using HP.Authentication.Infrastructure.DependencyInjection.Extensions.Database;
using HP.Authentication.Infrastructure.DependencyInjection.Extensions.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HP.Authentication.Infrastructure.DependencyInjection.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection InfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseExtension(configuration);
            services.AddIdentityExtension(configuration);
            services.AddLocalizationExtensions();
            services.AddCommonExtensions();
            return services;
        }
    }
}
