using be_authenticationApplication.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace be_authenticationApplication.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection ApplicationService(this IServiceCollection services)
        {
            services.AddMediatRExtensions();

            return services;
        }
    }
}
