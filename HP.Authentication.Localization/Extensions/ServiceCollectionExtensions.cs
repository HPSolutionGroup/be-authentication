using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HP.Authentication.Localization.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonLocalization(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJsonLocalizationService, JsonLocalizationService>();
            return services;
        }
    }
}
