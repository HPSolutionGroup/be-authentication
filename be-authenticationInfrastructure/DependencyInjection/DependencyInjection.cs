using be_authenticationInfrastructure.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be_authenticationInfrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection InfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseExtension(configuration);

            return services;
        }
    }
}
