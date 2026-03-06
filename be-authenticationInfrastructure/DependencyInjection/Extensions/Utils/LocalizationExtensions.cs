using Microsoft.Extensions.DependencyInjection;
using be_localization.Extensions;

namespace be_authenticationInfrastructure.DependencyInjection.Extensions.Utils
{
    public static class LocalizationExtensions
    {
        public static IServiceCollection AddLocalizationExtensions(this IServiceCollection services)
        {
            services.AddJsonLocalization();
            return services;
        }
    }
}
