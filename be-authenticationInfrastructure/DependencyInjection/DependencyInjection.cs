using be_authenticationInfrastructure.DependencyInjection.Extensions.Authentication;
using be_authenticationInfrastructure.DependencyInjection.Extensions.Common;
using be_authenticationInfrastructure.DependencyInjection.Extensions.Database;
using be_authenticationInfrastructure.DependencyInjection.Extensions.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be_authenticationInfrastructure.DependencyInjection
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
