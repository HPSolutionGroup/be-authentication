using HP.Authentication.Localization.Extensions;
using Microsoft.Extensions.DependencyInjection;
namespace HP.Authentication.Infrastructure.DependencyInjection.Extensions.Utils
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
