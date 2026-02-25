using be_localization.Abstractions;
using be_localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace be_localization.Extensions
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
