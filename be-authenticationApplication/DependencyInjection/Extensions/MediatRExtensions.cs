using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace be_authenticationApplication.DependencyInjection.Extensions
{
    public static class MediatRExtensions
    {
        public static IServiceCollection AddMediatRExtensions(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.GetExecutingAssembly());
            });
            return services;
        }
    }
}
