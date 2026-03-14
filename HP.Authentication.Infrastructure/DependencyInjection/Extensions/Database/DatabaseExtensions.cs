using HP.Authentication.Application.Abstractions.Repository;
using HP.Authentication.Infrastructure.Data;
using HP.Authentication.Infrastructure.Integrations.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HP.Authentication.Infrastructure.DependencyInjection.Extensions.Database
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseExtension(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext
            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Default"),
                    b => b.MigrationsAssembly(typeof(DatabaseExtensions).Assembly.FullName)
                ), ServiceLifetime.Scoped);

            // Đăng ký Repository và UnitOfWork
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
    }
}
